using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using MVC.Models;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Configuraiton pour recevoir les ApplicationConfiguration du AppConfig ...
        private ApplicationConfiguration _applicationConfiguration { get; }

        // Voice le IOptionsSnapshot qui importe dans l'object la configuraiton du AppConfig.
        public HomeController(ILogger<HomeController> logger, IOptionsSnapshot<ApplicationConfiguration> options)
        {
            _applicationConfiguration = options.Value;
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
