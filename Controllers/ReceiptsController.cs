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
using Expenses_Manager.Models.enums;
using System.Globalization;

namespace Expenses_Manager.Controllers
{
    public class ReceiptsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReceiptsController(ApplicationDbContext context)
        {
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

        // GET: Receipts
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var receipts = await _context.Receipt.Where(r => r.UserId == GetUserId().Result).ToListAsync();

            foreach(Receipt receipt in receipts)
            {
                var expensesList = _context.Expense.Where(e => e.ReceiptId == receipt.Id).ToListAsync();

                var hasPedingPayments = expensesList.Result.Any(e => e.Status == PaymentStatus.Pending);
                receipt.PendingPayments = hasPedingPayments;

                var totalExpensesValue = expensesList.Result.Sum(e => e.Value);
                receipt.TotalValue = Math.Round(totalExpensesValue, 2);
            }

            return View(receipts);
        }

        // GET: Receipts/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Receipt == null)
            {
                return NotFound();
            }

            var receipt = await _context.Receipt
                .FirstOrDefaultAsync(m => m.Id == id);
            if (receipt == null)
            {
                return NotFound();
            }
            

            var expensesList = _context.Expense.Where(e => e.ReceiptId == id).ToListAsync();
            receipt.Expenses = expensesList.Result;

            var hasPedingPayments = expensesList.Result.Any(e => e.Status == PaymentStatus.Pending);
            receipt.PendingPayments = hasPedingPayments;

            var totalExpensesValue = expensesList.Result.Sum(e => e.Value);
            receipt.TotalValue = Math.Round(totalExpensesValue, 2);

            return View(receipt);
        }

        // GET: Receipts/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Receipts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,UserId,Month,Year,TotalValue,PendingPayments")] Receipt receipt)
        {
            if (ModelState.IsValid)
            {
                receipt.UserId = GetUserId().Result;

                _context.Add(receipt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(receipt);
        }

        // GET: Receipts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Receipt == null)
            {
                return NotFound();
            }

            var receipt = await _context.Receipt
                .FirstOrDefaultAsync(m => m.Id == id);
            if (receipt == null)
            {
                return NotFound();
            }

            return View(receipt);
        }

        // POST: Receipts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Receipt == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Receipt'  is null.");
            }
            var receipt = await _context.Receipt.FindAsync(id);
            if (receipt != null)
            {
                _context.Receipt.Remove(receipt);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReceiptExists(int id)
        {
          return _context.Receipt.Any(e => e.Id == id);
        }
    }
}
