using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Expenses_Manager.Data;
using Expenses_Manager.Models;
using Microsoft.AspNetCore.Authorization;
using Expenses_Manager.Models.Util;
using Expenses_Manager.Models.Enums;
using Syncfusion.EJ2.Linq;

namespace Expenses_Manager.Controllers
{
    public class PaymentMethodsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentMethodsController(ApplicationDbContext context)
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

        // Ordenar resultados
        [Authorize]
        public async Task<IActionResult> OrdenedIndex([Bind("PaymentMethods,PaymentMethodsOrderType,PaymentMethodsFilterType,PaymentMethodsFilterValue,PaymentMethodsOrder")] PaymentMethodsQuery paymentMethod)
        {
            if(ModelState.IsValid)
            {
                List<PaymentMethod> getPaymentMethods = await _context.PaymentMethod.Where(x => x.UserId == GetUserId().Result).ToListAsync();

                if(paymentMethod.PaymentMethodsOrderType == PaymentMethodOrderType.ValorLimite)
                {
                    if (paymentMethod.PaymentMethodsOrder == ResultsOrder.Descendente)
                        getPaymentMethods = getPaymentMethods.OrderBy(x => x.LimitValue).OrderByDescending(x => x.LimitValue).ToList();
                    else
                        getPaymentMethods = getPaymentMethods.OrderBy(x => x.LimitValue).ToList();
                }
                else if (paymentMethod.PaymentMethodsOrderType == PaymentMethodOrderType.Fechamento)
                {
                    if (paymentMethod.PaymentMethodsOrder == ResultsOrder.Descendente)
                        getPaymentMethods = getPaymentMethods.OrderBy(x => x.ReceiptClosingDay).OrderByDescending(x => x.ReceiptClosingDay).ToList();
                    else
                        getPaymentMethods = getPaymentMethods.OrderBy(x => x.ReceiptClosingDay).ToList();
                }
                else
                {
                    if (paymentMethod.PaymentMethodsOrder == ResultsOrder.Descendente)
                        getPaymentMethods = getPaymentMethods.OrderBy(x => x.CurrentValue).OrderByDescending(x => x.CurrentValue).ToList();
                    else
                        getPaymentMethods = getPaymentMethods.OrderBy(x => x.CurrentValue).ToList();
                }

                if(paymentMethod.PaymentMethodsFilterValue != String.Empty && paymentMethod.PaymentMethodsFilterValue != null)
                {
                    if (paymentMethod.PaymentMethodsFilterType == PaymentMethodFilterType.ValorLimite)
                        getPaymentMethods = getPaymentMethods.Where(x => x.LimitValue == Convert.ToDouble(paymentMethod.PaymentMethodsFilterValue)).ToList();
                    else if(paymentMethod.PaymentMethodsFilterType == PaymentMethodFilterType.ValorAtual)
                        getPaymentMethods = getPaymentMethods.Where(x => x.CurrentValue == Convert.ToDouble(paymentMethod.PaymentMethodsFilterValue)).ToList();
                    else if(paymentMethod.PaymentMethodsFilterType == PaymentMethodFilterType.Bandeira)
                        getPaymentMethods = getPaymentMethods.Where(x => x.Flag == paymentMethod.PaymentMethodsFilterValue).ToList();
                    else if(paymentMethod.PaymentMethodsFilterType == PaymentMethodFilterType.Tipo)
                        getPaymentMethods = getPaymentMethods.Where(x => x.Type.ToString() == paymentMethod.PaymentMethodsFilterValue).ToList();
                }

                paymentMethod.PaymentMethods = getPaymentMethods;
                return View(paymentMethod);
            }
            return View();
        }

        // GET: PaymentMethods
        [Authorize]
        public async Task<IActionResult> Index()
        {
            List<PaymentMethod> getPaymentMethods = await _context.PaymentMethod.Where(c => c.UserId == GetUserId().Result && c.Flag != "Dinheiro").ToListAsync();
            PaymentMethodsQuery paymentMethod = new PaymentMethodsQuery();
            paymentMethod.PaymentMethods = getPaymentMethods;

            return View(paymentMethod);
        }

        // GET: PaymentMethods/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PaymentMethod == null)
            {
                return NotFound();
            }

            var paymentMethod = await _context.PaymentMethod
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentMethod == null)
            {
                return NotFound();
            }

            return View(paymentMethod);
        }

        // GET: PaymentMethods/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: PaymentMethods/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,HolderName,Number,SecurityCode,Flag,Type,ReceiptClosingDay,LimitValue,CurrentValue")] PaymentMethod paymentMethod)
        {
            if (ModelState.IsValid)
            {
                paymentMethod.UserId = GetUserId().Result;

                _context.Add(paymentMethod);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(paymentMethod);
        }

        // GET: PaymentMethods/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PaymentMethod == null)
            {
                return NotFound();
            }

            var paymentMethod = await _context.PaymentMethod.FindAsync(id);
            if (paymentMethod == null)
            {
                return NotFound();
            }
            paymentMethod.UserId = GetUserId().Result;

            return View(paymentMethod);
        }

        // POST: PaymentMethods/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,HolderName,Number,SecurityCode,Flag,Type,ReceiptClosingDay,LimitValue,CurrentValue")] PaymentMethod paymentMethod)
        {
            if (id != paymentMethod.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    paymentMethod.UserId = GetUserId().Result;

                    _context.Update(paymentMethod);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentMethodExist(paymentMethod.Id))
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
            return View(paymentMethod);
        }

        // GET: PaymentMethods/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PaymentMethod == null)
            {
                return NotFound();
            }

            var paymentMethod = await _context.PaymentMethod
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentMethod == null)
            {
                return NotFound();
            }

            return View(paymentMethod);
        }

        // POST: PaymentMethods/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PaymentMethod == null)
            {
                return Problem("Entity set 'ApplicationDbContext.PaymentMethod'  is null.");
            }
            var paymentMethod = await _context.PaymentMethod.FindAsync(id);
            if (paymentMethod != null)
            {
                if(_context.Expense.Any(x => x.PaymentMethodId == id))
                {
                    paymentMethod.StatusMessage = "Ainda há despezas vinculadas a esse método de pagamento";
                    return View(paymentMethod);
                }

                _context.PaymentMethod.Remove(paymentMethod);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentMethodExist(int id)
        {
          return _context.PaymentMethod.Any(e => e.Id == id);
        }
    }
}
