using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.NodeServices;
using Paycompute.Models;
using Paycompute.Services;

namespace Paycompute.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBranchService _branchService;

        public HomeController(IBranchService branchService)
        {
            _branchService = branchService;
        }
        public IActionResult Index()
        {
            var branches = _branchService.GetAll().Select(branch => new HomeBranchViewModel
            {
              Address = branch.Address
            }).ToList();
            return View(branches);
        }
    

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
