using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Paycompute.Entity;
using Paycompute.Models;
using Paycompute.Services;
using RotativaCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paycompute.Controllers
{
    [Authorize(Roles ="Admin,Manager")]   
    public class PayController : Controller
    {
        private readonly IPayComputationService _payComputationService;
        private readonly IEmployeeService _employeeService;
        private readonly ITaxService _taxService;
        private readonly INationalInsuranceContributionService _nationalInsuranceContributionService;
        private readonly HostingEnvironment _hostingEnvironment;
        private decimal overtimeHrs;
        private decimal contractualEarnings;
        private decimal overtimeEarnings;
        private decimal totalEarnings;
        private decimal tax;
        private decimal unionFee;
        private decimal studentLoan;
        private decimal nationalInsurance;
        private decimal totalDeduction;

        public PayController(IPayComputationService payComputationService, 
                            IEmployeeService employeeService,
                            ITaxService taxService,
                             HostingEnvironment hostingEnvironment,
                            INationalInsuranceContributionService nationalInsuranceContributionService)
        {
            _payComputationService = payComputationService;
            _employeeService = employeeService;
            _taxService = taxService;
            _nationalInsuranceContributionService = nationalInsuranceContributionService;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public IActionResult Index(int? pageNumber)
        {
            var unfilteredPaymentRecords = _payComputationService.GetAll();

            var filteredPaymentRecords = (from payment in unfilteredPaymentRecords
                                          where ((TempData["FilterMonth"] == null || payment.PayMonth == (string)TempData["FilterMonth"])
                                                 && (TempData["FilterFirstName"] == null || payment.FullName.Contains((string)TempData["FilterFirstName"]))
                                                 && (TempData["FilterLastName"] == null || payment.FullName.Contains((string)TempData["FilterLastName"])))
                                          select payment).ToList();

            var paymentTable = (from employee in _employeeService.GetAll()
                                select employee).ToList();

            var filteredPaymentJoinedGender =
                (from pay in filteredPaymentRecords
                 join emp in paymentTable on pay.EmployeeId equals emp.Id into payEmp
                 from genderOrNull in payEmp.DefaultIfEmpty()
                 select new { paymnet = pay, Employee = genderOrNull }).ToList();

            PaymentRecordIndexViewModel paymentRecordIndexViewModel = new PaymentRecordIndexViewModel();

            var paymentIndexViewModelListItems = filteredPaymentJoinedGender.Select(pair => new PaymentRecordIndexViewModel.PaymentRecordListItem()
            {
                Id = pair.paymnet.Id,
                EmployeeId = pair.paymnet.EmployeeId,
                FullName = pair.paymnet.FullName,
                PayDate = pair.paymnet.PayDate,
                PayMonth = pair.paymnet.PayMonth,
                TaxYearId = pair.paymnet.TaxYearId,
                TaxYear = _payComputationService.GetTaxYearById(pair.paymnet.TaxYearId).YearOfTax,
                TotalEarnings = pair.paymnet.TotalEarnings,
                TotalDeduction = pair.paymnet.TotalDeduction,
                NetPayment = pair.paymnet.NetPayment,
                Employee = pair.paymnet.Employee,
                Gender = pair.Employee.Gender
            }).ToList();


            int countEmployeesWithTotalEarningsOver10K =
                (from paymentRecord in _payComputationService.GetAll()
                 group paymentRecord by paymentRecord.EmployeeId
                    into groupRes
                 select groupRes.Sum(pr => pr.TotalEarnings)).Count(sm => sm >= 10000);

            paymentRecordIndexViewModel.Items = paymentIndexViewModelListItems;
            paymentRecordIndexViewModel.CountEmployeesTotalEarningsOver10K = countEmployeesWithTotalEarningsOver10K;

            return View(paymentRecordIndexViewModel);
        }

        public async Task<IActionResult> Chart()
        {
            var payments = _payComputationService.GetAll().Select(pay => new PaymentChartViewModel
            {

                FullName = pay.FullName,
                TotalEarnings = pay.TotalEarnings,

            }).ToList();


            var comlumHeadrs = new string[]
              {

                "Full-Name",
                "Ernings"
              };

            Dictionary<string, int> hash = new Dictionary<string, int>();

            foreach (var e in payments)
            {
                if (hash.ContainsKey(e.FullName))
                {
                    hash[e.FullName] = hash[e.FullName] + (int)e.TotalEarnings;
                }
                else
                {
                    hash.Add(e.FullName, (int)e.TotalEarnings);
                }

            }

            var employeeRecords = (from employee in hash.Keys
                                   select new object[]
                                   {

                                            $"{employee}",
                                            $"{hash[employee]}", //Escaping ","
                                           
                                   }).ToList();

            // Build the file content
            var employeecsv = new StringBuilder();
            employeeRecords.ForEach(line =>
            {
                employeecsv.AppendLine(string.Join(",", line));
            });

            byte[] buffer = Encoding.ASCII.GetBytes($"{string.Join(",", comlumHeadrs)}\r\n{employeecsv.ToString()}");
            string path = this._hostingEnvironment.WebRootPath + "\\Files\\Pay.csv";
            Byte[] byteArray = Convert.FromBase64String(Convert.ToBase64String(buffer));
            Stream stream = new MemoryStream(byteArray);
            try
            {
                FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                stream.Position = 0;
                stream.CopyTo(fileStream);

                stream.Dispose();
                fileStream.Dispose();

            }
            catch (System.Exception e)
            {
                throw new Exception("error", e);
            }
            File(buffer, "text/csv", $"Employee.csv");

            return View();
        }

        [Authorize(Roles ="Admin")]
        public IActionResult Create()
        {
            ViewBag.employees = _employeeService.GetAllEmployeesForPayroll();
            ViewBag.taxYears = _payComputationService.GetAllTaxYear();
            var model = new PaymentRecordCreateViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(PaymentRecordCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var payrecord = new PaymentRecord()
                {
                    Id = model.Id,
                    EmployeeId = model.EmployeeId,
                    FullName = _employeeService.GetById(model.EmployeeId).FullName,
                    NiNo = _employeeService.GetById(model.EmployeeId).NationalInsuranceNo,
                    PayDate = model.PayDate,
                    PayMonth = model.PayMonth,
                    TaxYearId = model.TaxYearId,
                    TaxCode = model.TaxCode,
                    HourlyRate = model.HourlyRate,
                    HoursWorked = model.HoursWorked,
                    ContractualHours = model.ContractualHours,
                    OvertimeHours = overtimeHrs = _payComputationService.OvertimeHours(model.HoursWorked, model.ContractualHours),
                    ContractualEarnings = contractualEarnings =_payComputationService.ContractualEarnings(model.ContractualHours, model.HoursWorked, model.HourlyRate),
                    OvertimeEarnings = overtimeEarnings = _payComputationService.OvertimeEarnings(_payComputationService.OvertimeRate(model.HourlyRate), overtimeHrs),
                    TotalEarnings = totalEarnings =_payComputationService.TotalEarnings(overtimeEarnings, contractualEarnings),
                    Tax = tax =_taxService.TaxAmount(totalEarnings),
                    UnionFee = unionFee = _employeeService.UnionFees(model.EmployeeId),
                    SLC = studentLoan = _employeeService.StudentLoanRepaymentAmount(model.EmployeeId, totalEarnings),
                    NIC = nationalInsurance =_nationalInsuranceContributionService.NIContribution(totalEarnings),
                    TotalDeduction = totalDeduction = _payComputationService.TotalDeduction(tax, nationalInsurance, studentLoan, unionFee),
                    NetPayment = _payComputationService.NetPay(totalEarnings, totalDeduction)
                };
                await _payComputationService.CreateAsync(payrecord);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.employees = _employeeService.GetAllEmployeesForPayroll();
            ViewBag.taxYears = _payComputationService.GetAllTaxYear();
            return View();
        }

        public IActionResult Detail(int id)
        {
            var paymentRecord = _payComputationService.GetById(id);
            if (paymentRecord == null)
            {
                return NotFound();
            }

            var model = new PaymentRecordDetailViewModel()
            {
                Id = paymentRecord.Id,
                EmployeeId = paymentRecord.EmployeeId,
                FullName = paymentRecord.FullName,
                NiNo = paymentRecord.NiNo,
                PayDate = paymentRecord.PayDate,
                PayMonth = paymentRecord.PayMonth,
                TaxYearId = paymentRecord.TaxYearId,
                Year = _payComputationService.GetTaxYearById(paymentRecord.TaxYearId).YearOfTax,
                TaxCode = paymentRecord.TaxCode,
                HourlyRate = paymentRecord.HourlyRate,
                HoursWorked = paymentRecord.HoursWorked,
                ContractualHours = paymentRecord.ContractualHours,
                OvertimeHours = paymentRecord.OvertimeHours,
                OvertimeRate = _payComputationService.OvertimeRate(paymentRecord.HourlyRate),
                ContractualEarnings = paymentRecord.ContractualEarnings,
                OvertimeEarnings = paymentRecord.OvertimeEarnings,
                Tax = paymentRecord.Tax,
                NIC = paymentRecord.NIC,
                UnionFee = paymentRecord.UnionFee,
                SLC = paymentRecord.SLC,
                TotalEarnings = paymentRecord.TotalEarnings,
                TotalDeduction = paymentRecord.TotalDeduction,
                Employee = paymentRecord.Employee,
                TaxYear = paymentRecord.TaxYear,
                NetPayment = paymentRecord.NetPayment
            };
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Payslip(int id)
        {
            var paymentRecord = _payComputationService.GetById(id);
            if (paymentRecord == null)
            {
                return NotFound();
            }

            var model = new PaymentRecordDetailViewModel()
            {
                Id = paymentRecord.Id,
                EmployeeId = paymentRecord.EmployeeId,
                FullName = paymentRecord.FullName,
                NiNo = paymentRecord.NiNo,
                PayDate = paymentRecord.PayDate,
                PayMonth = paymentRecord.PayMonth,
                TaxYearId = paymentRecord.TaxYearId,
                Year = _payComputationService.GetTaxYearById(paymentRecord.TaxYearId).YearOfTax,
                TaxCode = paymentRecord.TaxCode,
                HourlyRate = paymentRecord.HourlyRate,
                HoursWorked = paymentRecord.HoursWorked,
                ContractualHours = paymentRecord.ContractualHours,
                OvertimeHours = paymentRecord.OvertimeHours,
                OvertimeRate = _payComputationService.OvertimeRate(paymentRecord.HourlyRate),
                ContractualEarnings = paymentRecord.ContractualEarnings,
                OvertimeEarnings = paymentRecord.OvertimeEarnings,
                Tax = paymentRecord.Tax,
                NIC = paymentRecord.NIC,
                UnionFee = paymentRecord.UnionFee,
                SLC = paymentRecord.SLC,
                TotalEarnings = paymentRecord.TotalEarnings,
                TotalDeduction = paymentRecord.TotalDeduction,
                Employee = paymentRecord.Employee,
                TaxYear = paymentRecord.TaxYear,
                NetPayment = paymentRecord.NetPayment
            };
            return View(model);
        }

        public IActionResult GeneratePayslipPdf(int id)
        {
            var payslip = new ActionAsPdf("Payslip", new { id = id })
            {
                FileName = "payslip.pdf"
            };
            return payslip;
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(PaymentRecordSearchViewModel model)
        {
            // fill TempData with filter fields to be passed to Index action
            TempData["FilterFirstName"] = model.FirstName;
            TempData["FilterLastName"] = model.LastName;

            if (model.Month != "-- Select Month ---")
            {
                TempData["FilterMonth"] = model.Month;
            }

            return RedirectToAction("Index");
        }
    }


}
