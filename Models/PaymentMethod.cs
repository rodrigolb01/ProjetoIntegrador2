using Expenses_Manager.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expenses_Manager.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        [DisplayName("Usuário")]
        public string? HolderName { get; set; }
        [Required]
        [DisplayName("Número")]
        public string? Number { get; set; } = "0000000000000000";
        [Required]
        [DisplayName("Código de segurança")]
        public string? SecurityCode { get; set; } = "000";
        [Required]
        [DisplayName("Bandeira")]
        public string? Flag { get; set; } = "";
        [Required]
        [DisplayName("Tipo")]
        public PaymentType Type { get; set; } = PaymentType.Credito;
        [Required]
        [DisplayName("Fechamento da fatura")]
        public DateTime ReceiptClosingDay { get; set; } = DateTime.Now;
        [DisplayName("Limite")]
        public double? LimitValue { get; set; } = 0;
        [DisplayName("Valor atual")]
        public double CurrentValue { get; set; } = 0;
        [NotMapped]
        [TempData]
        public string? StatusMessage { get; set; }

        public PaymentMethod()
        {

        }
    }
}
