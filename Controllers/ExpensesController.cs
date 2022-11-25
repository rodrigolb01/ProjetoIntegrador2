using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Expenses_Manager.Data;
using Expenses_Manager.Models;
using Microsoft.AspNetCore.Authorization;
using Expenses_Manager.Models.Enums;

namespace Expenses_Manager.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
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
            var categoriesList = _context.Category.OrderBy(s => s.Name).Select(x => new { Id = x.Id, Value = x.Name, UserId = x.UserId });
            var paymentMethodsList = _context.PaymentMethod.OrderBy(s => s.Flag).Select(x => new {Id = x.Id, Value = x.Type != PaymentType.Dinheiro ? x.Type.ToString() + " " + x.Flag + " terminado em " + x.Number.Substring(x.Number.Length - 2) : x.Flag, UserId = x.UserId});

            var userCaregoriesList = categoriesList.Where(e => e.UserId == GetUserId().Result);
            var userPaymentMethodsList = paymentMethodsList.Where(p => p.UserId == GetUserId().Result);

            Expense expenseModel = new Expense();
            expenseModel.AvailableCategories = new SelectList(userCaregoriesList, "Id", "Value");
            expenseModel.AvailablePaymentMethods = new SelectList(userPaymentMethodsList, "Id", "Value");

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
                var categoriesList = _context.Category.OrderBy(s => s.Name).Select(x => new { Id = x.Id, Value = x.Name, UserId = x.UserId });
                var paymentMethodsList = _context.PaymentMethod.OrderBy(s => s.Flag).Select(x => new { Id = x.Id, Value = x.Type != PaymentType.Dinheiro ? x.Type.ToString() + " " + x.Flag + " terminado em " + x.Number.Substring(x.Number.Length - 2) : x.Flag, UserId = x.UserId });

                var userCaregoriesList = categoriesList.Where(e => e.UserId == GetUserId().Result);
                var userPaymentMethodsList = paymentMethodsList.Where(p => p.UserId == GetUserId().Result);

                expense.AvailableCategories = new SelectList(userCaregoriesList, "Id", "Value");
                expense.AvailablePaymentMethods = new SelectList(userPaymentMethodsList, "Id", "Value");

                int currentReceiptId = (int)TempData["currentReceiptId"];
                //salva id da receita atual para futuras operacoes
                TempData["currentReceiptId"] = currentReceiptId;
                Receipt currentReceipt = _context.Receipt.FirstOrDefaultAsync(x => x.Id == currentReceiptId).Result;
                PaymentMethod currentPaymentMethod = _context.PaymentMethod.FirstOrDefaultAsync(x => x.Id == expense.PaymentMethodId).Result;
                bool receiptClosed = expense.date > currentPaymentMethod.ReceiptClosingDay;

                //Em caso de: data > fechamento da receita
                //Aviso: Esta receita ja esta fechada, lancar na receita do proximo mes
                if (receiptClosed)
                {
                    expense.StatusMessage = "Fatura fechada para o método de pagamento escolhido. Despeza deve ser lançada na fatura do próximo mês";
                    return View(expense);
                }

                //Se a data do pagamento for diferente do mes em questao:
                //Aviso: usuario deve lancar despeza na fatura do mes correspondente
                if (expense.date.Month != currentReceipt.Month && receiptClosed == false)
                {
                    expense.StatusMessage = "Despeza deve ser lançada na fatura do mês correspondente";
                    return View(expense);
                }

                //Em caso de: compra.valor > (cartao.limite - cartao.valorAtual)
                //Aviso: Saldo insuficiente
                if (currentPaymentMethod.Type == PaymentType.Credito)
                {
                    if (expense.Value > currentPaymentMethod.LimitValue - currentPaymentMethod.CurrentValue)
                    {
                        expense.StatusMessage = "O método de pagamento escolhido possui saldo insuficiente";
                        return View(expense);
                    }
                    else
                        currentPaymentMethod.CurrentValue += expense.Value;
                }

                expense.UserId = GetUserId().Result;
                expense.ReceiptId = currentReceiptId;
                expense.CategoryName = _context.Category.FirstOrDefaultAsync(x => x.Id == expense.CategoryId).Result.Name;
                expense.PaymentMethodName = "id: " + currentPaymentMethod.Id + " " +  currentPaymentMethod.Type.ToString() + " " + currentPaymentMethod.Flag + " terminado em " + currentPaymentMethod.Number.Substring(currentPaymentMethod.Number.Length - 2);

                _context.Add(expense);

                //update total cost of receipt
                Receipt thisReceipt = _context.Receipt.FirstOrDefaultAsync(x => x.Id == expense.ReceiptId).Result;
                thisReceipt.TotalValue += expense.Value;

                _context.Update(thisReceipt);

                await _context.SaveChangesAsync();
                return Redirect("/Receipts/Details/"+currentReceiptId);
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

            var categoriesList = _context.Category.OrderBy(s => s.Name).Select(x => new { Id = x.Id, Value = x.Name, UserId = x.UserId });
            var paymentMethodsList = _context.PaymentMethod.OrderBy(s => s.Flag).Select(x => new { Id = x.Id, Value = x.Type != PaymentType.Dinheiro ? x.Type.ToString() + " " + x.Flag + " terminado em " + x.Number.Substring(x.Number.Length - 2) : x.Flag, UserId = x.UserId });

            var userCaregoriesList = categoriesList.Where(e => e.UserId == GetUserId().Result);
            var userPaymentMethodsList = paymentMethodsList.Where(p => p.UserId == GetUserId().Result);

            expense.AvailableCategories = new SelectList(userCaregoriesList, "Id", "Value");
            expense.AvailablePaymentMethods = new SelectList(userPaymentMethodsList, "Id", "Value");
            expense.UserId = GetUserId().Result;

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
                    var categoriesList = _context.Category.OrderBy(s => s.Name).Select(x => new { Id = x.Id, Value = x.Name, UserId = x.UserId });
                    var paymentMethodsList = _context.PaymentMethod.OrderBy(s => s.Flag).Select(x => new { Id = x.Id, Value = x.Type != PaymentType.Dinheiro ? x.Type.ToString() + " " + x.Flag + " terminado em " + x.Number.Substring(x.Number.Length - 2) : x.Flag, UserId = x.UserId });

                    var userCaregoriesList = categoriesList.Where(e => e.UserId == GetUserId().Result);
                    var userPaymentMethodsList = paymentMethodsList.Where(p => p.UserId == GetUserId().Result);

                    expense.AvailableCategories = new SelectList(userCaregoriesList, "Id", "Value");
                    expense.AvailablePaymentMethods = new SelectList(userPaymentMethodsList, "Id", "Value");

                    int currentReceiptId = (int)TempData["currentReceiptId"];
                    //salva id da receita atual para futuras operacoes
                    TempData["currentReceiptId"] = currentReceiptId;
                    Receipt currentReceipt = _context.Receipt.FirstOrDefaultAsync(x => x.Id == currentReceiptId).Result;
                    PaymentMethod currentPaymentMethod = _context.PaymentMethod.FirstOrDefaultAsync(x => x.Id == expense.PaymentMethodId).Result;
                    bool receiptClosed = expense.date > currentPaymentMethod.ReceiptClosingDay;

                    //Em caso de: data > fechamento da receita
                    //Aviso: Esta receita ja esta fechada, lancar na receita do proximo mes
                    if (receiptClosed)
                    {
                        expense.StatusMessage = "Fatura fechada para o método de pagamento escolhido. Despeza deve ser lançada na fatura do próximo mês";
                        return View(expense);
                    }

                    //Se a data do pagamento for diferente do mes em questao:
                    //Aviso: usuario deve lancar despeza na fatura do mes correspondente
                    if (expense.date.Month != currentReceipt.Month && receiptClosed == false)
                    {
                        expense.StatusMessage = "Despeza deve ser lançada na fatura do mês correspondente";
                        return View(expense);
                    }                  

                    //Em caso de: compra.valor > (cartao.limite - cartao.valorAtual)
                    //Aviso: Saldo insuficiente
                    if (currentPaymentMethod.Type == PaymentType.Credito)
                    {
                        //como nao sabemos qual era o saldo do cartao antes de adicionar a despeza, iremos recalcular todo o saldo
                        //somando todas as despezas que usaram o cartao excluindo a atual                       
                        List<Expense> expenses = _context.Expense.Where(x => x.PaymentMethodId == currentPaymentMethod.Id).ToListAsync().Result;

                        double paymentMethodCurrentValue = 0; ;

                        foreach (Expense e in expenses)
                            if (e.Id != id)
                                paymentMethodCurrentValue += e.Value;

                        currentPaymentMethod.CurrentValue = paymentMethodCurrentValue;

                        if (expense.Value > currentPaymentMethod.LimitValue - currentPaymentMethod.CurrentValue)
                        {
                            expense.StatusMessage = "O método de pagamento escolhido possui saldo insuficiente";
                            return View(expense);
                        }
                        else
                            currentPaymentMethod.CurrentValue += expense.Value;
                    }

                    expense.UserId = GetUserId().Result;
                    expense.ReceiptId = (int)TempData["currentReceiptId"];
                    expense.CategoryName = _context.Category.FirstOrDefaultAsync(x => x.Id == expense.CategoryId).Result.Name;
                    expense.PaymentMethodName = "id: " + currentPaymentMethod.Id + " " + currentPaymentMethod.Type.ToString() + " " + currentPaymentMethod.Flag + " terminado em " + currentPaymentMethod.Number.Substring(currentPaymentMethod.Number.Length - 2);

                    _context.Update(expense);
                    await _context.SaveChangesAsync();

                    //update total value of the receipt
                    Receipt thisReceipt = _context.Receipt.FirstOrDefaultAsync(x => x.Id == expense.ReceiptId).Result;
                    List<Expense> thisReceiptExpenses = _context.Expense.Where(x => x.ReceiptId == thisReceipt.Id).ToListAsync().Result;

                    double totalValue = 0;
                    foreach (Expense e in thisReceiptExpenses)
                    {
                        totalValue += e.Value;
                    }

                    thisReceipt.TotalValue = totalValue;

                    _context.Update(thisReceipt);
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
                int expenseId = (int)TempData["currentReceiptId"];
                //salva id da receita atual para futuras operacoes
                TempData["currentReceiptId"] = expense.ReceiptId;
                return Redirect("/Receipts/Details/" + expenseId);
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
                //update total value of the receipt
                Receipt thisReceipt = _context.Receipt.FirstOrDefaultAsync(x => x.Id == expense.ReceiptId).Result;
                thisReceipt.TotalValue = thisReceipt.TotalValue - expense.Value;

                _context.Update(thisReceipt);

                _context.Expense.Remove(expense);
            }          

            await _context.SaveChangesAsync();

            int expenseId = (int)TempData["currentReceiptId"];
            return Redirect("/Receipts/Details/" + expenseId);
        }

        private bool ExpenseExists(int id)
        {
          return _context.Expense.Any(e => e.Id == id);
        }
    }
}
