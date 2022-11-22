using Expenses_Manager.Data;
using Expenses_Manager.Models;
using Expenses_Manager.Models.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using System.Diagnostics;

namespace Expenses_Manager.Controllers
{
    public class UIController : Controller
    {
        private readonly ILogger<UIController> _logger;
        private readonly ApplicationDbContext _context;

        public UIController(ILogger<UIController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        //Recupera o Id do usuario que esta logado
        [Authorize]
        public async Task<string> GetUserId()
        {
            var loggedUserName = User.Identity.Name;
            var getUser = _context.Users.FirstOrDefaultAsync(x => x.UserName == loggedUserName);

            return getUser.Result.Id;
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
        public async Task <IActionResult> Home()
        {
            var lineData = await _context.Receipt
                .OrderBy(x => x.Month)
                .Where(x => x.UserId == GetUserId().Result)
                .Select(x => new LineData 
                { 
                    xValue = new DateTime(x.Year, x.Month, 01), 
                    yValue = x.TotalValue
                })
                .ToListAsync();

            return View(lineData);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}