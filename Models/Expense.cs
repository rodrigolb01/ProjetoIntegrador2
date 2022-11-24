using Expenses_Manager.Models.enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expenses_Manager.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public int ReceiptId { get; set; } = 0;
        [DisplayName("Dia")]
        public DateTime date { get; set; } = DateTime.Now;
        [DisplayName("Descrição")]
        public string? Name { get; set; }
        [DisplayName("Valor")]
        public double Value { get; set; } = 0;
        [DisplayName("Método de pagamento")]
        public int PaymentMethodId { get; set; }
        [DisplayName("Status")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        [DisplayName("Parcelas")]
        public int? Installments { get; set; } = 1;
        [DisplayName("Categoria")]
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
