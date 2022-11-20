// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Expenses_Manager.Data;
using Expenses_Manager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Expenses_Manager.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly ApplicationDbContext _context;

        public DeletePersonalDataModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;

            var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-Expenses_Manager-4FDCB869-85CC-48D7-83C9-BB00B4BAFD6F;Trusted_Connection=True;MultipleActiveResultSets=true")
              .EnableSensitiveDataLogging()
              .Options;
            _context = new ApplicationDbContext(contextOptions);
        }

        [BindProperty]
        public InputModel Input { get; set; }
   
        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            //deletes address data
            var userData = _context.UserData.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id).Result;
            if( userData != null)
            {
                _context.UserData.Remove(userData);
                await _context.SaveChangesAsync();
            }

            //if user have any data left delete all and close account
            List<Expense> userExpenses = _context.Expense.AsNoTracking().Where(x => x.UserId == user.Id).ToListAsync().Result;
            if (userExpenses.Any() || userExpenses != null)
            {
                foreach (Expense expense in userExpenses)
                {
                    _context.Expense.Remove(expense);
                }
                await _context.SaveChangesAsync();
            }

            List<Category> userCategories = _context.Category.AsNoTracking().Where(x => x.UserId == user.Id).ToListAsync().Result;
            if(userCategories.Any() || userCategories != null)
            {
                foreach (Category category in userCategories)
                {
                    _context.Category.Remove(category);
                }
                await _context.SaveChangesAsync();
            }
           

            List<Receipt> userReceipts = _context.Receipt.AsNoTracking().Where(x => x.UserId == user.Id).ToListAsync().Result;
            if(userReceipts.Any() || userReceipts != null)
            {
                foreach(Receipt receipt in userReceipts)
                {
                    _context.Receipt.Remove(receipt);
                }
                await _context.SaveChangesAsync();
            }

            List<Card> userCards = _context.Card.AsNoTracking().Where(x => x.UserId == user.Id).ToListAsync().Result;
            if(userCards.Any() || userCards != null)
            {
                foreach(Card card in userCards)
                {
                    _context.Card.Remove(card);
                }
                await _context.SaveChangesAsync();
            }

            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }
    }
}
