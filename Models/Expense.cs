using Expenses_Manager.Models.enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expenses_Manager.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public int ReceiptId { get; set; } = 0;
        public DateTime date { get; set; } = DateTime.Now;
        public string? Name { get; set; }
        public double Value { get; set; } = 0;
        public int PaymentMethodId { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public int? Installments { get; set; } = 1;
        public int CategoryId { get; set; }
        [NotMapped]
        public SelectList? AvailableCategories { get; set; } 
        [NotMapped]
        public SelectList? AvailablePaymentMethods { get; set; }

        public Expense()
        {

        }
    }
}
