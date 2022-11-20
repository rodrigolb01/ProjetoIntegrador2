// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Expenses_Manager.Data;
using Expenses_Manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Expenses_Manager.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;


        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;

            var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-Expenses_Manager-4FDCB869-85CC-48D7-83C9-BB00B4BAFD6F;Trusted_Connection=True;MultipleActiveResultSets=true")
                .EnableSensitiveDataLogging()
                .Options;
            

            _context = new ApplicationDbContext(contextOptions);
        }

        public string ProfileUrl { get; set; }
        public string Username { get; set; }
        [TempData]
        public string StatusMessage { get; set; }
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
            [Display(Name = "Profile Picture Url")]
            public string ProfileUrl { get; set; }
        }

        //Recupera o Id do usuario que esta logado
        [Authorize]
        public async Task<string> GetLoggedUserId()
        {
            var loggedUserName = User.Identity.Name;
            var getUser = _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserName == loggedUserName);

            return getUser.Result.Id;
        }

        //Recupera os dados do usuario
        [Authorize]
        public async Task<UserData> GetLoggedUserData()
        {
            var userData = await _context.UserData.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == GetLoggedUserId().Result);
            if(userData == null)
                return null;
            
            return userData;
        }

        private async Task LoadAsync(IdentityUser user)
        {
            string userName = await _userManager.GetUserNameAsync(user);
            string phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            string defaultProfilePicUrl = "https://photogeeksteven.files.wordpress.com/2014/06/default-user-icon-profile.png";
            string profileUrl;

            var currentUserData = GetLoggedUserData().Result;
            if (currentUserData != null)
                profileUrl = currentUserData.ProfilePicture;
            else
                profileUrl = defaultProfilePicUrl;

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                ProfileUrl = profileUrl
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            var profileUserData = GetLoggedUserData().Result;

            UserData userData = new UserData()
            {
                UserId = user.Id,
                Name = user.Email,
                State = "",
                City = "",
                AddressLine = "",
                ProfilePicture = "https://photogeeksteven.files.wordpress.com/2014/06/default-user-icon-profile.png"
            };

            if (Input.ProfileUrl != String.Empty)
                userData.ProfilePicture = Input.ProfileUrl;

            if (profileUserData != null) // reuse data and update
            {
                userData.Id = profileUserData.Id;
                userData.UserId = profileUserData.UserId;
                userData.Name = profileUserData.Name;
                userData.AddressLine = profileUserData.AddressLine;
                userData.City = profileUserData.City;
                userData.State = profileUserData.State;

                _context.Update(userData);
            }
            else // add new just for profile pocture
                _context.Add(userData);

            await _context.SaveChangesAsync();

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
