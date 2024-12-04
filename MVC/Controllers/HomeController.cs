using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using MVC.Models;
using System.Diagnostics;

// Requis pour l'injection de la dépendance pour l'AppConfig
using Microsoft.Extensions.Options;

// Requis pour l'injection de la dépendance pour le Flag Management
using Microsoft.FeatureManagement;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Configuration pour recevoir les ApplicationConfiguration du AppConfig ...
        private ApplicationConfiguration _applicationConfiguration { get; }

        // Configuration pour recevoir les Flags
        private IFeatureManager _featureManager { get; }

        // Voir le IOptionsSnapshot qui importe dans l'object la configuraiton du AppConfig.
        // Ainsi que le IFeatureManager pour la gestion des Flags
        public HomeController(ILogger<HomeController> logger, IOptionsSnapshot<ApplicationConfiguration> options, IFeatureManager featureManager)
        {
            _applicationConfiguration = options.Value;
            _logger = logger;
            _featureManager = featureManager;
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
