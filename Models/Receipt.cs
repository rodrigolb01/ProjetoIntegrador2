using Expenses_Manager.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expenses_Manager.Models
{
    public class Receipt
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        [DisplayName("Mês")]
        public int Month { get; set; } = DateTime.Now.Month;
        [DisplayName("Ano")]
        public int Year { get; set; } = DateTime.Now.Year;
        [DisplayName("Total da fatura")]
        public double TotalValue { get; set; } = 0;
        [DisplayName("Pagamentos pendentes")]
        public bool PendingPayments { get; set; } = false;
        [NotMapped]
        public List<Expense> Expenses { get; set; } = new List<Expense>();
        [NotMapped]
        [DisplayName("Ordenar por")]
        public OrderType OrderType { get; set; } = OrderType.Dia;
        [NotMapped]
        [DisplayName("Filtrar por")]
        public FilterType FilterType { get; set; } = FilterType.Nada;
        [NotMapped]
        [DisplayName("Valor do filtro")]
        public string? FilterValue { get; set; }
        [NotMapped]
        [DisplayName("Em ordem")]
        public Query? Query { get; set; }

        public Receipt()
        {

        }
    }
}
