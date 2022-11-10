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

namespace Expenses_Manager.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Expenses/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Expense == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // GET: Expenses/Create
        [Authorize]
        public IActionResult Create()
        {
            var categoriesList = _context.Category.OrderBy(s => s.Name).Select(x => new { Id = x.Id, Value = x.Name });
            var paymentMethodsList = _context.Card.OrderBy(s => s.Flag).Select(x => new { Id = x.Id, Value = x.Flag });

            Expense expenseModel = new Expense();
            expenseModel.AvailableCategories = new SelectList(categoriesList, "Id", "Value");
            expenseModel.AvailablePaymentMethods = new SelectList(paymentMethodsList, "Id", "Value");

            return View(expenseModel);
        }

        // POST: Expenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,UserId,ReceiptId,date,Name,Value,PaymentMethodId,Status,Installments,CategoryId")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                expense.UserId = user.Result.Id;

                _context.Add(expense);
                await _context.SaveChangesAsync();
                return Redirect("/Receipts/Index");
            }
            return View(expense);
        }

        // GET: Expenses/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Expense == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }
            var categoriesList = _context.Category.OrderBy(s => s.Name).Select(x => new { Id = x.Id, Value = x.Name });
            var paymentMethodsList = _context.Card.OrderBy(s => s.Flag).Select(x => new { Id = x.Id, Value = x.Flag });

            expense.AvailableCategories = new SelectList(categoriesList, "Id", "Value");
            expense.AvailablePaymentMethods = new SelectList(paymentMethodsList, "Id", "Value");

            return View(expense);
        }

        // POST: Expenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,ReceiptId,date,Name,Value,PaymentMethodId,Status,Installments,CategoryId")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                    expense.UserId = user.Id;

                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect("/Receipts/Index");
            }
            return View(expense);
        }

        // GET: Expenses/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Expense == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Expense == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Expense'  is null.");
            }
            var expense = await _context.Expense.FindAsync(id);
            if (expense != null)
            {
                _context.Expense.Remove(expense);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
          return _context.Expense.Any(e => e.Id == id);
        }
    }
}
