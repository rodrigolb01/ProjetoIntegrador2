﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Expenses_Manager.Data;
using Expenses_Manager.Models;
using Microsoft.AspNetCore.Authorization;
using Expenses_Manager.Models.Util;
using Expenses_Manager.Models.Enums;
using Syncfusion.EJ2.Linq;

namespace Expenses_Manager.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Retorna para a fatura
        [Authorize]
        public async Task<IActionResult> BackToList()
        {
            int expenseId = (int)TempData["currentReceiptId"];
            return Redirect("/Receipts/Details/" + expenseId);
        }

        //Recupera o Id do usuario que esta logado
        [Authorize]
        public async Task<string> GetUserId()
        {
            var loggedUserName = User.Identity.Name;
            var getUser = _context.Users.FirstOrDefaultAsync(x => x.UserName == loggedUserName);

            return getUser.Result.Id;
        }

        // Ordenar resultados
        [Authorize]
        public async Task<IActionResult> OrdenedIndex([Bind("Categories,CategoriesOrderType,CategoriesFilterType,CategoriesFilterValue,CategoriesOrder")] CategoriesQuery category)
        {
            if (ModelState.IsValid)
            {
                List<Category> getCategories = await _context.Category.Where(x => x.UserId == GetUserId().Result).ToListAsync();

                if(category.CategoriesOrder == ResultsOrder.Descendente)
                    getCategories = getCategories.OrderBy(x => x.Name).OrderByDescending(x => x.Name).ToList();
                else
                    getCategories = getCategories.OrderBy(x => x.Name).ToList();

                if (category.CategoriesFilterValue != String.Empty && category.CategoriesFilterValue != null)
                    getCategories = getCategories.Where(x => x.Name == category.CategoriesFilterValue).ToList();

                category.Categories = getCategories;
                return View(category);
            }
            else
                return View();
        }

        // GET: Categories
        [Authorize]
        public async Task<IActionResult> Index()
        {
            List<Category> getCategories = await _context.Category.Where(x => x.UserId == GetUserId().Result).ToListAsync();
            CategoriesQuery categoriesQuery = new CategoriesQuery();
            categoriesQuery.Categories = getCategories;

            return View(categoriesQuery);
        }

        // GET: Categories/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Category == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,UserId,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.UserId = GetUserId().Result;

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Category == null)
            {
                return NotFound();
            }

            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            category.UserId = GetUserId().Result;

            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Name")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.UserId = GetUserId().Result;

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        // GET: Categories/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Category == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Category == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Category'  is null.");
            }
            var category = await _context.Category.FindAsync(id);
            if (category != null)
            {
                _context.Category.Remove(category);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
          return _context.Category.Any(e => e.Id == id);
        }
    }
}
