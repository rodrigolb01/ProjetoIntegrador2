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
            var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-Expenses_Manager-4FDCB869-85CC-48D7-83C9-BB00B4BAFD6F;Trusted_Connection=True;MultipleActiveResultSets=true")
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

            UserData userData = new UserData()
            {
                UserId = user.Id,
                Name = user.Email,
                State = "",
                City = "",
                AddressLine = "",
                ProfilePicture = ""
            };

            var currentUserData = _context.UserData.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id).Result;         

            if (currentUserData != null) // if already existing data then reuse it
            {
                userData.Id = currentUserData.Id;

                if (currentUserData.City != null || currentUserData.City != string.Empty)
                    userData.City = currentUserData.City;
                if (currentUserData.State != null || currentUserData.State != string.Empty)
                    userData.State = currentUserData.State;
                if (currentUserData.AddressLine != null || currentUserData.AddressLine != string.Empty)
                    userData.AddressLine = currentUserData.AddressLine;
                if (currentUserData.ProfilePicture != null || currentUserData.ProfilePicture != string.Empty)
                    userData.ProfilePicture = currentUserData.ProfilePicture;
            }

            userData.City = Input.City;
            userData.State = Input.State;
            userData.AddressLine = Input.AddressLine;

            if(currentUserData == null)
                _context.Add(userData);
            else
                _context.Update(userData);

            await _context.SaveChangesAsync();

            StatusMessage = "Address data updated successfuly";

            return Page();
        }
    }
}
