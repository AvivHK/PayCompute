using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.NodeServices;
using Paycompute.Entity;
using Paycompute.Models;
using Paycompute.Services;
using PayCompute.External;

namespace Paycompute.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly HostingEnvironment _hostingEnvironment;
        private readonly IPayComputationService _payComputationService;
        public EmployeeController(IEmployeeService employeeService, IPayComputationService payComputationService, HostingEnvironment hostingEnvironment)
        {
            _employeeService = employeeService;
            _hostingEnvironment = hostingEnvironment;
            _payComputationService = payComputationService;

        }

        public IActionResult Index(int? pageNumber)
        {
            var unfilteredEmployees = _employeeService.GetAll();
            var filteredEmployees = (from employee in unfilteredEmployees
                                     where (TempData["FilterCity"] == null || employee.City == (string)TempData["FilterCity"])
                                     && (TempData["FilterEmployeeNo"] == null || employee.EmployeeNo == (string)TempData["FilterEmployeeNo"])
                                     && (TempData["FilterFirstName"] == null || employee.FullName.Contains((string)TempData["FilterFirstName"]))
                                     && (TempData["FilterLastName"] == null || employee.FullName.Contains((string)TempData["FilterLastName"]))
                                     select employee).ToList();

            var latestPaymentRecords = (from paymentRecord in _payComputationService.GetAll()
                                        group paymentRecord by paymentRecord.EmployeeId
                                        into groups
                                        select groups.OrderByDescending(pr => pr.PayDate).First()).ToList();
            //join
            var filteredEmployeesJoinedLatestPaymentRecords =
                 (from emp in filteredEmployees
                  join pay in latestPaymentRecords on emp.Id equals pay.EmployeeId into empPays
                  from payOrNull in empPays.DefaultIfEmpty() // for left outer join
                  select new { Employee = emp, PaymentRecord = payOrNull }).ToList();


            var employeeIndexViewModelList = filteredEmployeesJoinedLatestPaymentRecords.Select(pair => new EmployeeIndexViewModel
            {
                Id = pair.Employee.Id,
                EmployeeNo = pair.Employee.EmployeeNo,
                ImageUrl = pair.Employee.ImageUrl,
                FullName = pair.Employee.FullName,
                Gender = pair.Employee.Gender,
                Designation = pair.Employee.Designation,
                City = pair.Employee.City,
                DateJoined = pair.Employee.DateJoined,
                LatestPaymentRecordNetPayment = pair.PaymentRecord != null ? pair.PaymentRecord.NetPayment : 0
            }).ToList();
            int pageSize = 4;
            return View(EmployeeListPagination<EmployeeIndexViewModel>.Create(employeeIndexViewModelList, pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(EmployeeSearchViewModel model)
        {
            // fill TempData with filter fields to be passed to Index action
            TempData["FilterCity"] = model.City;
            TempData["FilterFirstName"] = model.FirstName;
            TempData["FilterLastName"] = model.LastName;
            TempData["FilterEmployeeNo"] = model.EmployeeNo;

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Chart()
        {
            var employees = _employeeService.GetAll().Select(employee => new EmployeeChartViewModel
            {
                             
                FullName = employee.FullName,
                Designation = employee.Designation,

            }).ToList();
            

            var comlumHeadrs = new string[]
              {
              
                "Position",
                "Count"
              };

            Dictionary<string, int> hash = new Dictionary<string, int>();

            foreach (var e in employees){
                if (hash.ContainsKey(e.Designation))
                {
                    hash[e.Designation] = hash[e.Designation] + 1;
                }
                else
                {
                    hash.Add(e.Designation, 1);
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
            string path = this._hostingEnvironment.WebRootPath + "\\Files\\test.csv" ;
            Byte[] byteArray = Convert.FromBase64String(Convert.ToBase64String(buffer));
            Stream stream = new MemoryStream(byteArray);
            try
            {
                FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);

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

        [HttpGet]
        public IActionResult Create()
        {
            var model = new EmployeeCreateViewModel();
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken] //Prevents cross-site Request Forgery Attacks
        public async Task<IActionResult> Create(EmployeeCreateViewModel model)
        {
            if(ModelState.IsValid)
            {
                var employee = new Employee
                {
                    Id = model.Id,
                    EmployeeNo = model.EmployeeNo,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    FullName = model.FullName,
                    Gender = model.Gender,
                    Email = model.Email,
                    DOB = model.DOB,
                    DateJoined = model.DateJoined,
                    NationalInsuranceNo = model.NationalInsuranceNo,
                    PaymentMethod = model.PaymentMethod,
                    StudentLoan = model.StudentLoan,
                    UnionMember = model.UnionMember,
                    Address = model.Address,
                    City = model.City,
                    Phone = model.Phone,
                    Postcode = model.Postcode,
                    Designation = model.Designation,
                    
                };
                if (model.ImageUrl != null && model.ImageUrl.Length > 0)
                {
                    var uploadDir = @"images/employee";
                    var fileName = Path.GetFileNameWithoutExtension(model.ImageUrl.FileName);
                    var extension = Path.GetExtension(model.ImageUrl.FileName);
                    var webRootPath = _hostingEnvironment.WebRootPath;
                    fileName = DateTime.UtcNow.ToString("yymmssfff") + fileName + extension;
                    var path = Path.Combine(webRootPath, uploadDir, fileName);
                    await model.ImageUrl.CopyToAsync(new FileStream(path, FileMode.Create));
                    employee.ImageUrl = "/" + uploadDir + "/" + fileName;
                }
                await _employeeService.CreateAsync(employee);
                var FB = new FacebookApi();
                FB.PublishMessage($"Welcome {employee.FullName} to PayCompute!");
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        
        public IActionResult Edit(int id)
        {
            var employee = _employeeService.GetById(id);
            if (employee == null)
            {
                return NotFound();
            }
            var model = new EmployeeEditViewModel()
            {
                Id = employee.Id,
                EmployeeNo = employee.EmployeeNo,
                FirstName = employee.FirstName,
                MiddleName = employee.MiddleName,
                LastName = employee.LastName,                
                Gender = employee.Gender,
                Email = employee.Email,
                DOB = employee.DOB,
                DateJoined = employee.DateJoined,
                NationalInsuranceNo = employee.NationalInsuranceNo,
                PaymentMethod = employee.PaymentMethod,
                StudentLoan = employee.StudentLoan,
                UnionMember = employee.UnionMember,
                Address = employee.Address,
                City = employee.City,
                Phone = employee.Phone,
                Postcode = employee.Postcode,
                Designation = employee.Designation,
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var employee = _employeeService.GetById(model.Id);
                if (employee == null)
                {
                    return NotFound();
                }
                employee.EmployeeNo = model.EmployeeNo;
                employee.FirstName = model.FirstName;
                employee.LastName = model.LastName;
                employee.MiddleName = model.MiddleName;
                employee.NationalInsuranceNo = model.NationalInsuranceNo;
                employee.Gender = model.Gender;
                employee.Email = model.Email;
                employee.DOB = model.DOB;
                employee.DateJoined = model.DateJoined;
                employee.Phone = model.Phone;
                employee.Designation = model.Designation;
                employee.PaymentMethod = model.PaymentMethod;
                employee.StudentLoan = model.StudentLoan;
                employee.UnionMember = model.UnionMember;
                employee.Address = model.Address;
                employee.City = model.City;
                employee.Postcode = model.Postcode;
                if (model.ImageUrl != null && model.ImageUrl.Length > 0)
                {
                    var uploadDir = @"images/employee";
                    var fileName = Path.GetFileNameWithoutExtension(model.ImageUrl.FileName);
                    var extension = Path.GetExtension(model.ImageUrl.FileName);
                    var webRootPath = _hostingEnvironment.WebRootPath;
                    fileName = DateTime.UtcNow.ToString("yymmssfff") + fileName + extension;
                    var path = Path.Combine(webRootPath, uploadDir, fileName);
                    await model.ImageUrl.CopyToAsync(new FileStream(path, FileMode.Create));
                    employee.ImageUrl = "/" + uploadDir + "/" + fileName;
                }
                await _employeeService.UpdateAsync(employee);
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        [HttpGet]
        public IActionResult Detail(int id)
        {
            var employee = _employeeService.GetById(id);
            if (employee == null)
            {
                return NotFound();
            }
            EmployeeDetailViewModel model = new EmployeeDetailViewModel()
            {
                Id = employee.Id,
                EmployeeNo = employee.EmployeeNo,
                FullName = employee.FullName,
                Gender = employee.Gender,
                DOB = employee.DOB,
                DateJoined = employee.DateJoined,
                Designation = employee.Designation,
                NationalInsuranceNo = employee.NationalInsuranceNo,
                Phone = employee.Phone,
                Email = employee.Email,
                PaymentMethod = employee.PaymentMethod,
                StudentLoan = employee.StudentLoan,
                UnionMember = employee.UnionMember,
                Address = employee.Address,
                City = employee.City,
                ImageUrl = employee.ImageUrl,
                Postcode = employee.Postcode
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var employee = _employeeService.GetById(id);
            if (employee ==null)
            {
                return NotFound();
            }
            var model = new EmployeeDeleteViewModel()
            {
                Id = employee.Id,
                FullName = employee.FullName
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(EmployeeDeleteViewModel model)
        {
            await _employeeService.Delete(model.Id);
            return RedirectToAction(nameof(Index));
        }

    }
}
