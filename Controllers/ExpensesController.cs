using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Expenses_Manager.Data;
using Expenses_Manager.Models;
using Microsoft.AspNetCore.Authorization;
using Expenses_Manager.Models.Enums;
using Expenses_Manager.Models.enums;

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
            var getUser = _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserName == loggedUserName);

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

            Expense expense = new Expense();
            expense.AvailableCategories = new SelectList(userCaregoriesList, "Id", "Value");
            expense.AvailablePaymentMethods = new SelectList(userPaymentMethodsList, "Id", "Value");

            return View(expense);
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
                var getCategories = _context.Category.OrderBy(s => s.Name).Select(x => new { Id = x.Id, Value = x.Name, UserId = x.UserId });
                var getPaymentMethods = _context.PaymentMethod.OrderBy(s => s.Flag).Select(x => new { Id = x.Id, Value = x.Type != PaymentType.Dinheiro ? x.Type.ToString() + " " + x.Flag + " terminado em " + x.Number.Substring(x.Number.Length - 2) : x.Flag, UserId = x.UserId });

                var userCaregoriesList = getCategories.Where(e => e.UserId == GetUserId().Result);
                var userPaymentMethodsList = getPaymentMethods.Where(p => p.UserId == GetUserId().Result);

                expense.AvailableCategories = new SelectList(userCaregoriesList, "Id", "Value");
                expense.AvailablePaymentMethods = new SelectList(userPaymentMethodsList, "Id", "Value");

                int currentReceiptId = (int)TempData["currentReceiptId"];
                //salva id da receita atual para futuras operacoes
                TempData["currentReceiptId"] = currentReceiptId;

                Receipt currentReceipt = _context.Receipt.FirstOrDefaultAsync(x => x.Id == currentReceiptId).Result;
                PaymentMethod currentPaymentMethod = _context.PaymentMethod.FirstOrDefaultAsync(x => x.Id == expense.PaymentMethodId).Result;
                bool isReceiptClosed = expense.date.Day >= currentPaymentMethod.ReceiptClosingDay.Day;

                //Em caso de: data > fechamento da receita
                //Aviso: Esta receita ja esta fechada, lancar na receita do proximo mes
                if (isReceiptClosed && currentPaymentMethod.Type != PaymentType.Dinheiro)
                {
                    expense.StatusMessage = "Fatura fechada para o método de pagamento escolhido. Despeza deve ser lançada na fatura do próximo mês!";
                    return View(expense);
                }

                //Se a data do pagamento for diferente do mes em questao:
                //Aviso: usuario deve lancar despeza na fatura do mes correspondente
                if (expense.date.Month != currentReceipt.Month && isReceiptClosed == false)
                {
                    expense.StatusMessage = "Despeza deve ser lançada na fatura do mês correspondente!";
                    return View(expense);
                }

                //Em caso de: compra.valor > (cartao.limite - cartao.valorAtual)
                //Aviso: Saldo insuficiente, escolher outro metodo de pagamento
                if (currentPaymentMethod.Type != PaymentType.Dinheiro)
                {
                    if (expense.Value > currentPaymentMethod.LimitValue - currentPaymentMethod.CurrentValue)
                    {
                        expense.StatusMessage = "O método de pagamento escolhido possui saldo insuficiente";
                        return View(expense);
                    }
                    else
                    {
                        double val = (double)expense.Value;
                        currentPaymentMethod.CurrentValue += val;
                    }
                }

                //Em caso de: parcelas > 1 && pagamento em credito
                //Para cada parcela, verificar se ha uma fatura em aberto para o proximo mes, se nao criar novas faturas ate cobrir as parcelas
                if(expense.Installments > 1 && currentPaymentMethod.Type == PaymentType.Credito)
                {
                    int currentMonth = expense.date.Month;
                    int currentYear = expense.date.Year;
                    int finalMonth = 0;
                    int finalYear = 0;

                    if(((int)currentMonth + expense.Installments) > 12)
                    {
                        finalMonth = (int)(currentMonth + expense.Installments);
                        finalMonth = finalMonth - 12;

                        finalYear = currentYear + 1;
                    }
                    else
                    {
                        finalMonth = (int)(currentMonth + expense.Installments);
                        finalYear = currentYear;
                    }

                    DateTime currentDate = new DateTime(currentYear, currentMonth, expense.date.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    DateTime finalDate = new DateTime(finalYear, finalMonth, expense.date.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    
                    while(currentDate <= finalDate)
                    {
                        Expense installment = new Expense()
                        {
                            UserId = GetUserId().Result,
                            ReceiptId = 0,
                            Name = expense.Name,
                            date = currentDate,
                            Value = Math.Round((double)(expense.Value / expense.Installments), 2),
                            PaymentMethodId = expense.PaymentMethodId,
                            PaymentMethodName = "id: " + currentPaymentMethod.Id + " " + currentPaymentMethod.Type.ToString() + " " + currentPaymentMethod.Flag + " terminado em " + currentPaymentMethod.Number.Substring(currentPaymentMethod.Number.Length - 2),
                            Status = PaymentStatus.Pendente,
                            Installments = expense.Installments,
                            CategoryId = expense.CategoryId,
                            CategoryName = _context.Category.FirstOrDefaultAsync(x => x.Id == expense.CategoryId).Result.Name,
                    };
                        string userId = GetUserId().Result;
                        Receipt receipt = _context.Receipt.Where(x => x.UserId == userId).FirstOrDefaultAsync(x => x.Month == currentDate.Month).Result;
                        if (receipt != null)
                        {
                            installment.ReceiptId = receipt.Id;
                            double val = (double)installment.Value;
                            receipt.TotalValue += val;
                        }
                        else
                        {
                            double val = (double)installment.Value;
                            Receipt newReceipt = new Receipt()
                            {
                                UserId = GetUserId().Result,
                                Month = currentDate.Month,
                                Year = currentDate.Year,
                                Date = currentDate,
                                TotalValue = val,
                                PendingPayments = true
                            };

                            _context.Receipt.Add(newReceipt);
                            await _context.SaveChangesAsync();

                            int newReceiptId = _context.Receipt.Where(x => x.UserId == userId).FirstOrDefaultAsync(x => x.Month == currentDate.Month).Result.Id;

                            installment.ReceiptId = newReceiptId;
                        }

                        _context.Add(installment);
                        await _context.SaveChangesAsync();

                        int nextMonth = currentDate.Month + 1;

                        if(nextMonth > 12)
                        {
                            nextMonth = nextMonth - 12;

                            currentDate = new DateTime(currentDate.Year + 1, nextMonth, expense.date.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        }
                        else
                        {
                            currentDate = new DateTime(currentDate.Year, nextMonth, expense.date.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        }
                    }
                }
                else
                {
                    expense.UserId = GetUserId().Result;
                    expense.ReceiptId = currentReceiptId;
                    expense.CategoryName = _context.Category.FirstOrDefaultAsync(x => x.Id == expense.CategoryId).Result.Name;
                    expense.PaymentMethodName = "id: " + currentPaymentMethod.Id + " " + currentPaymentMethod.Type.ToString() + " " + currentPaymentMethod.Flag + " terminado em " + currentPaymentMethod.Number.Substring(currentPaymentMethod.Number.Length - 2);

                    _context.Add(expense);

                    //update total cost of receipt
                    Receipt thisReceipt = _context.Receipt.FirstOrDefaultAsync(x => x.Id == expense.ReceiptId).Result;
                    double val = (double)expense.Value;
                    thisReceipt.TotalValue += val;

                    _context.Update(thisReceipt);
                    await _context.SaveChangesAsync();
                }

                if (expense.Status == PaymentStatus.Pendente)
                {
                    currentReceipt.PendingPayments = true;
                    _context.Update(currentReceipt);
                    await _context.SaveChangesAsync();
                }

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

            var getUserCategories = _context.Category.AsNoTracking().OrderBy(s => s.Name).Select(x => new { Id = x.Id, Value = x.Name, UserId = x.UserId });
            var getUserPaymentMethods = _context.PaymentMethod.AsNoTracking().OrderBy(s => s.Flag).Select(x => new { Id = x.Id, Value = x.Type != PaymentType.Dinheiro ? x.Type.ToString() + " " + x.Flag + " terminado em " + x.Number.Substring(x.Number.Length - 2) : x.Flag, UserId = x.UserId });

            var userCaregoriesList = getUserCategories.AsNoTracking().Where(e => e.UserId == GetUserId().Result);
            var userPaymentMethodsList = getUserPaymentMethods.AsNoTracking().Where(p => p.UserId == GetUserId().Result);

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
                    var getUserCategories = _context.Category.AsNoTracking().OrderBy(s => s.Name).Select(x => new { Id = x.Id, Value = x.Name, UserId = x.UserId });
                    var getUserPaymentMethods = _context.PaymentMethod.AsNoTracking().OrderBy(s => s.Flag).Select(x => new { Id = x.Id, Value = x.Type != PaymentType.Dinheiro ? x.Type.ToString() + " " + x.Flag + " terminado em " + x.Number.Substring(x.Number.Length - 2) : x.Flag, UserId = x.UserId });

                    var userCaregoriesList = getUserCategories.Where(e => e.UserId == GetUserId().Result);
                    var userPaymentMethodsList = getUserPaymentMethods.Where(p => p.UserId == GetUserId().Result);

                    expense.AvailableCategories = new SelectList(userCaregoriesList, "Id", "Value");
                    expense.AvailablePaymentMethods = new SelectList(userPaymentMethodsList, "Id", "Value");

                    int currentReceiptId = (int)TempData["currentReceiptId"];
                    //salva id da receita atual para futuras operacoes
                    TempData["currentReceiptId"] = currentReceiptId;

                    Receipt currentReceipt = _context.Receipt.AsNoTracking().FirstOrDefaultAsync(x => x.Id == currentReceiptId).Result;
                    PaymentMethod currentPaymentMethod = _context.PaymentMethod.AsNoTracking().FirstOrDefaultAsync(x => x.Id == expense.PaymentMethodId).Result;
                    bool isReceiptClosed = expense.date.Day >= currentPaymentMethod.ReceiptClosingDay.Day;

                    //Em caso de: data > fechamento da receita
                    //Aviso: Esta receita ja esta fechada, lancar na receita do proximo mes
                    if (isReceiptClosed && currentPaymentMethod.Type != PaymentType.Dinheiro)
                    {
                        expense.StatusMessage = "Fatura fechada para o método de pagamento escolhido. Despeza deve ser lançada na fatura do próximo mês!";
                        return View(expense);
                    }

                    //Se a data do pagamento for diferente do mes em questao:
                    //Aviso: usuario deve lancar despeza na fatura do mes correspondente
                    if (expense.date.Month != currentReceipt.Month && isReceiptClosed == false)
                    {
                        expense.StatusMessage = "Despeza deve ser lançada na fatura do mês correspondente!";
                        return View(expense);
                    }                  

                    //Em caso de: compra.valor > (cartao.limite - cartao.valorAtual)
                    //Aviso: Saldo insuficiente
                    if (currentPaymentMethod.Type != PaymentType.Dinheiro)
                    {
                        //como nao sabemos qual era o saldo do cartao antes de adicionar a despeza, precisa recalcular todo o saldo
                        //somando todas as despezas que usaram o cartao excluindo a atual                       
                        List<Expense> currentReceiptExpenses = _context.Expense.AsNoTracking().Where(x => x.PaymentMethodId == currentPaymentMethod.Id).ToListAsync().Result;

                        double currentPaymentMethodRecalculatedValue = 0; ;

                        foreach (Expense e in currentReceiptExpenses)
                            if (e.Id != id)
                            {
                                double val = (double)e.Value;
                                currentPaymentMethodRecalculatedValue += val;
                            }

                        currentPaymentMethod.CurrentValue = currentPaymentMethodRecalculatedValue;

                        if (expense.Value > currentPaymentMethod.LimitValue - currentPaymentMethod.CurrentValue)
                        {
                            expense.StatusMessage = "O método de pagamento escolhido possui saldo insuficiente";
                            return View(expense);
                        }
                        else
                        {
                            double val = (double)expense.Value;
                            currentPaymentMethod.CurrentValue += val;
                        }
                    }

                    //Em caso de: parcelas > 1 && pagamento em credito
                    //Para cada parcela, verificar se ha uma fatura em aberto para o proximo mes, se nao criar novas faturas ate cobrir as parcelas
                    if (expense.Installments > 1 && currentPaymentMethod.Type == PaymentType.Credito)
                    {
                        int currentMonth = expense.date.Month;
                        int currentYear = expense.date.Year;
                        int finalMonth = 0;
                        int finalYear = 0;

                        if (((int)currentMonth + expense.Installments) > 12)
                        {
                            finalMonth = (int)(currentMonth + expense.Installments);
                            finalMonth = finalMonth - 12;
                            finalYear = currentYear + 1;
                        }
                        else
                        {
                            finalMonth = (int)(currentMonth + expense.Installments);
                            finalYear = currentYear;
                        }

                        DateTime currentDate = new DateTime(currentYear, currentMonth, expense.date.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        DateTime finalDate = new DateTime(finalYear, finalMonth, expense.date.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                        while (currentDate <= finalDate)
                        {
                            Expense installment = new Expense()
                            {
                                UserId = GetUserId().Result,
                                ReceiptId = 0,
                                Name = expense.Name,
                                date = currentDate,
                                Value = Math.Round((double)(expense.Value / expense.Installments), 2),
                                PaymentMethodId = expense.PaymentMethodId,
                                PaymentMethodName = "id: " + currentPaymentMethod.Id + " " + currentPaymentMethod.Type.ToString() + " " + currentPaymentMethod.Flag + " terminado em " + currentPaymentMethod.Number.Substring(currentPaymentMethod.Number.Length - 2),
                                Status = PaymentStatus.Pendente,
                                Installments = expense.Installments,
                                CategoryId = expense.CategoryId,
                                CategoryName = _context.Category.AsNoTracking().FirstOrDefaultAsync(x => x.Id == expense.CategoryId).Result.Name,
                            };
                            string userId = GetUserId().Result;
                            Receipt receipt = _context.Receipt.AsNoTracking().Where(x => x.UserId == userId).FirstOrDefaultAsync(x => x.Month == currentDate.Month).Result;
                            if (receipt != null)
                            {
                                installment.ReceiptId = receipt.Id;
                                double val = (double)installment.Value;
                                receipt.TotalValue += val;
                            }
                            else
                            {
                                double val = (double)installment.Value; 
                                Receipt newReceipt = new Receipt()
                                {
                                    UserId = GetUserId().Result,
                                    Month = currentDate.Month,
                                    Year = currentDate.Year,
                                    Date = currentDate,
                                    TotalValue = val,
                                    PendingPayments = true
                                };

                                _context.Receipt.Add(newReceipt);
                                await _context.SaveChangesAsync();

                                int newReceiptId = _context.Receipt.AsNoTracking().Where(x => x.UserId == userId).FirstOrDefaultAsync(x => x.Month == currentDate.Month).Result.Id;

                                installment.ReceiptId = newReceiptId;
                            }

                            _context.Add(installment);
                            await _context.SaveChangesAsync();

                            int nextMonth = currentDate.Month + 1;

                            if (nextMonth > 12)
                            {
                                nextMonth = nextMonth - 12;
                                currentDate = new DateTime(currentDate.Year + 1, nextMonth, expense.date.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                            }
                            else
                            {
                                currentDate = new DateTime(currentDate.Year, nextMonth, expense.date.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                            }
                        }
                    }
                    else
                    {
                        expense.UserId = GetUserId().Result;
                        expense.ReceiptId = (int)TempData["currentReceiptId"];
                        expense.CategoryName = _context.Category.AsNoTracking().FirstOrDefaultAsync(x => x.Id == expense.CategoryId).Result.Name;
                        expense.PaymentMethodName = "id: " + currentPaymentMethod.Id + " " + currentPaymentMethod.Type.ToString() + " " + currentPaymentMethod.Flag + " terminado em " + currentPaymentMethod.Number.Substring(currentPaymentMethod.Number.Length - 2);

                        _context.Update(expense);

                        await _context.SaveChangesAsync();

                        //update total value of the receipt
                        Receipt thisReceipt = _context.Receipt.AsNoTracking().FirstOrDefaultAsync(x => x.Id == expense.ReceiptId).Result;
                        List<Expense> thisReceiptExpenses = _context.Expense.AsNoTracking().Where(x => x.ReceiptId == thisReceipt.Id).ToListAsync().Result;

                        double totalValue = 0;
                        foreach (Expense e in thisReceiptExpenses)
                        {
                            double val = (double)e.Value;
                            totalValue += val;
                        }

                        thisReceipt.TotalValue = totalValue;

                        _context.Update(thisReceipt);
                        await _context.SaveChangesAsync();
                    }

                    if (expense.Status == PaymentStatus.Pendente)
                    {
                        currentReceipt.PendingPayments = true;
                        _context.Update(currentReceipt);
                        await _context.SaveChangesAsync();
                    }

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
                //atualizar valor atual da fatura
                Receipt thisReceipt = _context.Receipt.FirstOrDefaultAsync(x => x.Id == expense.ReceiptId).Result;
                double val = (double)expense.Value;
                thisReceipt.TotalValue = thisReceipt.TotalValue - val;

                _context.Update(thisReceipt);

                //atualizar valor atual do metodo de pagamento(credito)
                var currentPaymentMethod = _context.PaymentMethod.FirstOrDefaultAsync(x => x.Id == expense.PaymentMethodId).Result;

                if (currentPaymentMethod.Type == PaymentType.Credito)
                {
                    currentPaymentMethod.CurrentValue = currentPaymentMethod.CurrentValue - val;
                    _context.Update(currentPaymentMethod);
                }

                //antes de atualizar verificar se fora a despeza removida ainda ha alguma outra pendente
                if (expense.Status == PaymentStatus.Pendente)
                {
                    Receipt currentReceipt = _context.Receipt.FirstOrDefaultAsync(x => x.Id == expense.ReceiptId).Result;
                    List<Expense> currentReceiptExpenses = await _context.Expense.Where(x => x.ReceiptId == currentReceipt.Id && x.Id != expense.Id).ToListAsync();

                    if(!(currentReceiptExpenses.Any(x => x.Status == PaymentStatus.Pendente)))
                    {
                        currentReceipt.PendingPayments = false;
                        _context.Update(currentReceipt);
                    }

                    _context.Update(currentReceipt);
                    await _context.SaveChangesAsync();
                }

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
