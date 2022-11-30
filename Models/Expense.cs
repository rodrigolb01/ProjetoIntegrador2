using Expenses_Manager.Models.enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expenses_Manager.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public int ReceiptId { get; set; } = 0;
        [Required]
        [DisplayName("Dia")]
        public DateTime date { get; set; } = DateTime.Now;
        [Required]
        [DisplayName("Descrição")]
        public string? Name { get; set; }
        [Required]
        [DisplayName("Valor")]
        public double? Value { get; set; }
        [Required]
        [DisplayName("Método de pagamento")]
        public int PaymentMethodId { get; set; }
        [Required]
        [DisplayName("Status")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pendente;
        [DisplayName("Parcelas")]
        public int? Installments { get; set; } = 1;
        [Required]
        [DisplayName("Categoria")]
        public int CategoryId { get; set; }
        [NotMapped]
        public SelectList? AvailableCategories { get; set; }
        [NotMapped]
        public string? CategoryName { get; set; }
        [NotMapped]
        public SelectList? AvailablePaymentMethods { get; set; }
        [NotMapped]
        public string? PaymentMethodName { get; set; }
        [NotMapped]
        [TempData]
        public string? StatusMessage { get; set; }

        public Expense()
        {

        }
    }
}
