using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Expenses_Manager.Data;
using Expenses_Manager.Models;
using Microsoft.AspNetCore.Authorization;

namespace Expenses_Manager.Controllers
{
    public class UserDataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserData
        [Authorize]
        public async Task<IActionResult> Index()
        {
              return View(await _context.UserData.ToListAsync());
        }

        // GET: UserData/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.UserData == null)
            {
                return NotFound();
            }

            var userData = await _context.UserData
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userData == null)
            {
                return NotFound();
            }

            return View(userData);
        }

        // GET: UserData/Create
        [Authorize]
        public IActionResult Create()
        {
            //string userId = _context.Users.
            return View();
        }

        // POST: UserData/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Name,State,City,AddressLine,ProfilePicture")] UserData userData)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                userData.UserId = user.Result.Id;

                _context.Add(userData);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userData);
        }

        // GET: UserData/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.UserData == null)
            {
                return NotFound();
            }

            var userData = await _context.UserData.FindAsync(id);
            if (userData == null)
            {
                return NotFound();
            }
            return View(userData);
        }

        // POST: UserData/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Name,State,City,AddressLine,ProfilePicture")] UserData userData)
        {
            if (id != userData.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                    userData.UserId = user.Id;

                    _context.Update(userData);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserDataExists(userData.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(userData);
        }

        // GET: UserData/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.UserData == null)
            {
                return NotFound();
            }

            var userData = await _context.UserData
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userData == null)
            {
                return NotFound();
            }

            return View(userData);
        }

        // POST: UserData/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.UserData == null)
            {
                return Problem("Entity set 'ApplicationDbContext.UserData'  is null.");
            }
            var userData = await _context.UserData.FindAsync(id);
            if (userData != null)
            {
                _context.UserData.Remove(userData);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserDataExists(int id)
        {
          return _context.UserData.Any(e => e.Id == id);
        }
    }
}
