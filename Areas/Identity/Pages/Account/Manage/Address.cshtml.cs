using Expenses_Manager.Data;
using Expenses_Manager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Xml.Linq;

namespace Expenses_Manager.Areas.Identity.Pages.Account.Manage
{
    public class AddressModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AddressModel()
        {
            var builder = WebApplication.CreateBuilder();
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseSqlServer(connectionString)
               .EnableSensitiveDataLogging()
               .Options;

            _context = new ApplicationDbContext(contextOptions);
        }

        public class InputModel
        {
            public string City { get; set; }
            public string State { get; set; }
            public string AddressLine { get; set; }
        }

        [TempData]
        public string StatusMessage { get; set; }
        [BindProperty]
        public InputModel Input { get; set; }

      

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadPageAsync();
            return Page();
        }

        private async Task LoadPageAsync()
        {
            var user = _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name).Result;
            var userData = _context.UserData.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id).Result;
            string city = "";
            string state = "";
            string addressLine = "";

            if(userData != null)
            {
                if(userData.City != null || userData.City != string.Empty)
                    city = userData.City;
                if(userData.State != null || userData.State != string.Empty)      
                    state = userData.State;
                if(userData.AddressLine != null || userData.AddressLine != string.Empty)
                    addressLine = userData.AddressLine;
            }

            Input = new InputModel()
            {
                City = city,
                State = state,
                AddressLine = addressLine
            };
        }

        public async Task<IActionResult> OnPostChangeAddressAsync()
        {
            var user = _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name).Result;

            if(user == null)
            {
                return NotFound($"Unable to load user with ID '{user.Id}'.");
            }
            if (!ModelState.IsValid)
            {
                await LoadPageAsync();
                return Page();
            }

            var currentUserData = _context.UserData.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id).Result;

            currentUserData.City = Input.City;
            currentUserData.State = Input.State;
            currentUserData.AddressLine = Input.AddressLine;

            _context.Update(currentUserData);

            await _context.SaveChangesAsync();

            StatusMessage = "Endereço atualizado";

            return Page();
        }
    }
}
