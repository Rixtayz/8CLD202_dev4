using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using MVC.Models;
using System.Diagnostics;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private ApplicationConfiguration _applicationConfiguration { get; }

        public HomeController(ILogger<HomeController> logger, ApplicationConfiguration applicationConfiguration)
        {
            _applicationConfiguration = applicationConfiguration;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_applicationConfiguration);
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
