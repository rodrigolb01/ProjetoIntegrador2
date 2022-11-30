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
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
            public string UserName { get; set; }
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
            string userName = _context.UserData.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id) .Result.Name;
            string phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            string profileUrl = _context.UserData.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id).Result.ProfilePicture;

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                ProfileUrl = profileUrl,
                UserName = userName
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
            var profileUser = await _userManager.GetUserAsync(User);
            var profileUserData = GetLoggedUserData().Result;

            if (profileUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(profileUser);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(profileUser);

            //se usuario atualizou o telefone entao salvar
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(profileUser, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            profileUserData.ProfilePicture = Input.ProfileUrl;

            if (Input.UserName == null)
                profileUserData.Name = " ";
            else
                profileUserData.Name = Input.UserName;

            _context.Update(profileUserData);

            await _context.SaveChangesAsync();

            await _signInManager.RefreshSignInAsync(profileUser);
            StatusMessage = "Seu perfil foi atualizado";
            return RedirectToPage();
        }
    }
}
