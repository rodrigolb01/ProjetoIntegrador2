using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Expenses_Manager.Data;
using Expenses_Manager.Models;
using Microsoft.AspNetCore.Authorization;
using Expenses_Manager.Models.enums;
using Expenses_Manager.Models.Enums;
using Syncfusion.EJ2.Linq;

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

                var hasPedingPayments = expensesList.Result.Any(e => e.Status == PaymentStatus.Pendente);
                receipt.PendingPayments = hasPedingPayments;
            }

            return View(receipts);
        }

        // Reordenar resultados
        [Authorize]
        public async Task<IActionResult> OrdenedDetails([Bind("Id,Date,UserId,Month,Year,TotalValue,PendingPayments,Expenses,OrderType,FilterType,FilterValue,Query")] Receipt receipt)
        {
            List<Expense> expenses = new List<Expense>();
            receipt.Id = (int)TempData["currentReceiptId"];

            if (ModelState.IsValid)
            {
                switch(receipt.OrderType)
                {
                    case OrderType.Dia:
                        {
                            if(receipt.Query == Query.Descendente)
                                expenses = _context.Expense.Where(e => e.ReceiptId == receipt.Id).OrderBy(x => x.date.Day).OrderByDescending(x => x.date.Day).ToListAsync().Result;
                            else
                                expenses = _context.Expense.Where(e => e.ReceiptId == receipt.Id).OrderBy(x => x.date.Day).ToListAsync().Result;
                        }
                        break;
                    case OrderType.Valor:
                        {
                            if(receipt.Query == Query.Descendente)
                                expenses = _context.Expense.Where(e => e.ReceiptId == receipt.Id).OrderBy(x => x.Value).OrderByDescending(x => x.Value).ToListAsync().Result;
                            else
                                expenses = _context.Expense.Where(e => e.ReceiptId == receipt.Id).OrderBy(x => x.Value).ToListAsync().Result;
                        }
                        break;
                    default: expenses = _context.Expense.OrderBy(x => x.date.Day).Where(e => e.ReceiptId == receipt.Id).ToListAsync().Result; break;
                }


                if(receipt.FilterType != FilterType.Nada)
                {
                    switch (receipt.FilterType)
                    {
                        case FilterType.Dia: expenses = expenses.Where(x => x.date.Day == Convert.ToInt32(receipt.FilterValue)).ToList(); break;
                        case FilterType.Valor: expenses = expenses.Where(x => x.Value == Convert.ToDouble(receipt.FilterValue)).ToList(); break;
                        case FilterType.Status: expenses = expenses.Where(x => x.Status.ToString() == receipt.FilterValue).ToList(); break;
                        case FilterType.Pagamento: expenses = expenses.Where(x => x.PaymentMethodId == Convert.ToInt32(receipt.FilterValue)).ToList(); break;
                    }
                }      

                receipt.Expenses = expenses;

                var hasPedingPayments = expenses.Any(e => e.Status == PaymentStatus.Pendente);
                receipt.PendingPayments = hasPedingPayments;

                //salva a fatura sendo consultada atualmente para consulta das despezas
                TempData["currentReceiptId"] = receipt.Id;

                return View(receipt);
            }
            return View();
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
            
            List<Expense> expensesList = _context.Expense.Where(e => e.ReceiptId == id).ToListAsync().Result;
            foreach(Expense e in expensesList)
            {
                var currentPaymentMethod = _context.PaymentMethod.FirstOrDefaultAsync(x => x.Id == e.PaymentMethodId).Result;
                string currentPaymentMethodName = currentPaymentMethod.Type.ToString();

                if (currentPaymentMethod.Type != PaymentType.Dinheiro)
                    currentPaymentMethodName = currentPaymentMethod.Type.ToString() + " " + currentPaymentMethod.Flag + " terminado em " + currentPaymentMethod.Number.Substring(currentPaymentMethod.Number.Length - 2);

                e.PaymentMethodName = currentPaymentMethodName;

                var currentCategory = _context.Category.FirstOrDefaultAsync(x => x.Id == e.CategoryId).Result;
                e.CategoryName = currentCategory.Name;
            }
            receipt.Expenses = expensesList;

            var hasPedingPayments = expensesList.Any(e => e.Status == PaymentStatus.Pendente);
            receipt.PendingPayments = hasPedingPayments;

            //salva a fatura sendo consultada atualmente para consulta das despezas
            TempData["currentReceiptId"] = id;

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
        public async Task<IActionResult> Create([Bind("Id,Date,UserId,Month,Year,TotalValue,PendingPayments")] Receipt receipt)
        {
            if (ModelState.IsValid)
            {
                receipt.UserId = GetUserId().Result;
                receipt.Date = new DateTime(receipt.Year, receipt.Month, 1);//melhorar isso depois

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
                List<Expense> expensesList = _context.Expense.Where(x => x.ReceiptId == id).ToListAsync().Result;
                if(expensesList != null)
                {
                    foreach (Expense e in expensesList)
                    {
                        //extornar do cartao
                        var expensePaymentMethod = _context.PaymentMethod.FirstOrDefaultAsync(x => x.Id == e.PaymentMethodId).Result;
                        expensePaymentMethod.CurrentValue = Math.Round(expensePaymentMethod.CurrentValue - e.Value, 2);

                        if (expensePaymentMethod.CurrentValue < 0) // caso haja algum erro de calculo (melhorar depois)
                            expensePaymentMethod.CurrentValue = 0;

                        _context.Update(expensePaymentMethod);

                        _context.Remove(e);
                    }
                }

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
