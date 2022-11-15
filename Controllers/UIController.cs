using Expenses_Manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Expenses_Manager.Controllers
{
    public class UIController : Controller
    {
        private readonly ILogger<UIController> _logger;

        public UIController(ILogger<UIController> logger)
        {
            _logger = logger;
        }

        //Autenticar ou cadastrar usuario
        public IActionResult Index()
        {
            return View();
        }

        //Termos de uso
        public IActionResult Privacy()
        {
            return View();
        }

        //Menu principal
        [Authorize]
        public IActionResult Home()
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