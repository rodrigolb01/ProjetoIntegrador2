using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Expenses_Manager.Data;
using Expenses_Manager.Models;
using Microsoft.AspNetCore.Authorization;
using Expenses_Manager.Models.enums;
using Expenses_Manager.Models.Enums;
using Syncfusion.EJ2.Linq;
using Expenses_Manager.Models.Util;

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

        // Reordenar faturas
        [Authorize]
        public async Task<IActionResult> OrdenedIndex([Bind("Expenses,ReceiptsOrderType,ReceiptsFilterType,ReceiptsFilterValue,ReceiptsOrder")] ReceiptsQuery receipts)
        {
            if (ModelState.IsValid)
            {
                List<Receipt> getReceipts = await _context.Receipt.Where(x => x.UserId == GetUserId().Result).ToListAsync();

                if(receipts.ReceiptsOrderType == ReceiptOrderType.Mes)
                {
                    if (receipts.ReceiptsOrder == ResultsOrder.Descendente)
                        getReceipts = getReceipts.OrderBy(x => x.Month).OrderByDescending(x => x.Month).ToList();
                    else
                        getReceipts = getReceipts.OrderBy(x => x.Month).ToList();
                }
                else if(receipts.ReceiptsOrderType == ReceiptOrderType.Ano)
                {
                    if (receipts.ReceiptsOrder == ResultsOrder.Descendente)
                        getReceipts = getReceipts.OrderBy(x => x.Year).OrderByDescending(x => x.Year).ToList();
                    else
                        getReceipts = getReceipts.OrderBy(x => x.Year).ToList();
                }
                else if (receipts.ReceiptsOrderType == ReceiptOrderType.Valor)
                {
                    if (receipts.ReceiptsOrder == ResultsOrder.Descendente)
                        getReceipts = getReceipts.OrderBy(x => x.TotalValue).OrderByDescending(x => x.TotalValue).ToList();
                    else
                        getReceipts = getReceipts.OrderBy(x => x.TotalValue).ToList();
                }

                if(receipts.ReceiptsFilterValue != String.Empty && receipts.ReceiptsFilterValue != null )
                {
                    if (receipts.ReceiptsFilterType == ReceiptFilterType.Mes)
                        getReceipts = getReceipts.Where(x => x.Month == Convert.ToInt32(receipts.ReceiptsFilterValue)).ToList();
                    else if (receipts.ReceiptsFilterType == ReceiptFilterType.Ano)
                        getReceipts = getReceipts.Where(x => x.Year == Convert.ToInt32(receipts.ReceiptsFilterValue)).ToList();
                    else if (receipts.ReceiptsFilterType == ReceiptFilterType.Valor)
                        getReceipts = getReceipts.Where(x => x.TotalValue == Convert.ToDouble(receipts.ReceiptsFilterValue)).ToList();
                }

                receipts.Receipts = getReceipts;

                return View(receipts);
            }
            return View();
        }

        // GET: Receipts
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var getReceipts = await _context.Receipt.Where(r => r.UserId == GetUserId().Result).ToListAsync();

            foreach(Receipt receipt in getReceipts)
            {
                var expensesList = _context.Expense.Where(e => e.ReceiptId == receipt.Id).ToListAsync();

                var hasPedingPayments = expensesList.Result.Any(e => e.Status == PaymentStatus.Pendente);
                receipt.PendingPayments = hasPedingPayments;
            }

            ReceiptsQuery receipts = new ReceiptsQuery();
            receipts.Receipts = getReceipts;

            return View(receipts);
        }

        // Reordenar despezas
        [Authorize]
        public async Task<IActionResult> OrdenedDetails([Bind("Id,Date,UserId,Month,Year,TotalValue,PendingPayments,Expenses,OrderType,FilterType,FilterValue,Query")] Receipt receipt)
        {
            if (ModelState.IsValid)
            {
                List<Expense> getExpenses = new List<Expense>();
                receipt.Id = (int)TempData["currentReceiptId"];

                switch (receipt.ExpensesOrderType)
                {
                    case ExpenseOrderType.Dia:
                        {
                            if(receipt.ExpensesOrder == ResultsOrder.Descendente)
                                getExpenses = _context.Expense.Where(e => e.ReceiptId == receipt.Id).OrderBy(x => x.date.Day).OrderByDescending(x => x.date.Day).ToListAsync().Result;
                            else
                                getExpenses = _context.Expense.Where(e => e.ReceiptId == receipt.Id).OrderBy(x => x.date.Day).ToListAsync().Result;
                        }
                        break;
                    case ExpenseOrderType.Valor:
                        {
                            if(receipt.ExpensesOrder == ResultsOrder.Descendente)
                                getExpenses = _context.Expense.Where(e => e.ReceiptId == receipt.Id).OrderBy(x => x.Value).OrderByDescending(x => x.Value).ToListAsync().Result;
                            else
                                getExpenses = _context.Expense.Where(e => e.ReceiptId == receipt.Id).OrderBy(x => x.Value).ToListAsync().Result;
                        }
                        break;
                    case ExpenseOrderType.Descricao:
                        {
                            if (receipt.ExpensesOrder == ResultsOrder.Descendente)
                                getExpenses = _context.Expense.Where(e => e.ReceiptId == receipt.Id).OrderBy(x => x.Name).OrderByDescending(x => x.Name).ToListAsync().Result;
                            else
                                getExpenses = _context.Expense.Where(e => e.ReceiptId == receipt.Id).OrderBy(x => x.Name).ToListAsync().Result;

                        }
                        break;
                    case ExpenseOrderType.Categoria:
                        {
                            foreach (Expense e in getExpenses)
                            {
                                Category c = _context.Category.FirstOrDefaultAsync(x => x.Id == e.CategoryId).Result;
                                e.CategoryName = c.Name;
                            }

                            if (receipt.ExpensesOrder == ResultsOrder.Descendente)                        
                                getExpenses = _context.Expense.Where(e => e.ReceiptId == receipt.Id).OrderBy(x => x.CategoryName).OrderByDescending(x => x.CategoryName).ToListAsync().Result;
                            
                            else
                                getExpenses = _context.Expense.Where(e => e.ReceiptId == receipt.Id).OrderBy(x => x.CategoryName).ToListAsync().Result;
                        }
                        break;
                    default: getExpenses = _context.Expense.OrderBy(x => x.date.Day).Where(e => e.ReceiptId == receipt.Id).ToListAsync().Result; break;
                }

                if(receipt.ExpensesFilterValue != String.Empty && receipt.ExpensesFilterValue != null)
                {
                    if (receipt.ExpensesFilterType != ExpenseFilterType.Nada)
                    {
                        switch (receipt.ExpensesFilterType)
                        {
                            case ExpenseFilterType.Dia: getExpenses = getExpenses.Where(x => x.date.Day == Convert.ToInt32(receipt.ExpensesFilterValue)).ToList(); break;
                            case ExpenseFilterType.Valor: getExpenses = getExpenses.Where(x => x.Value == Convert.ToDouble(receipt.ExpensesFilterValue)).ToList(); break;
                            case ExpenseFilterType.Status: getExpenses = getExpenses.Where(x => x.Status.ToString() == receipt.ExpensesFilterValue).ToList(); break;
                            case ExpenseFilterType.Pagamento: getExpenses = getExpenses.Where(x => x.PaymentMethodId == Convert.ToInt32(receipt.ExpensesFilterValue)).ToList(); break;
                        }
                    }
                }

                receipt.Expenses = getExpenses;

                var hasPedingPayments = getExpenses.Any(e => e.Status == PaymentStatus.Pendente);
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
                        double val = (double)e.Value;
                        expensePaymentMethod.CurrentValue = Math.Round(expensePaymentMethod.CurrentValue - val, 2);

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
