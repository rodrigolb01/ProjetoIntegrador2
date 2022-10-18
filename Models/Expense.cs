using Expenses_Manager.Models.enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expenses_Manager.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ReceiptId { get; set; }
        public DateTime date { get; set; }
        public string? Name { get; set; }
        public double Value { get; set; }
        public int PaymentMethodId { get; set; }
        public PaymentStatus Status { get; set; }
        public int? Installments { get; set; }
        public int CategoryId { get; set; }
        [NotMapped]
        public SelectList? AvailableCategories { get; set; }
        [NotMapped]
        public SelectList? AvailablePaymentMethods { get; set; }
    }
}
