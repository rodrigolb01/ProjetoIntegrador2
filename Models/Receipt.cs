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
        public int Month { get; set; } = DateTime.Now.Month;
        public int Year { get; set; } = DateTime.Now.Year;
        public double TotalValue { get; set; } = 0;
        public bool PendingPayments { get; set; } = false;
        [NotMapped]
        public List<Expense> Expenses { get; set; } = new List<Expense>();
        [NotMapped]
        [DisplayName("Order")]
        public OrderType OrderType { get; set; } = OrderType.ByDay;
        [NotMapped]
        [DisplayName("Filter")]
        public FilterType FilterType { get; set; } = FilterType.None;
        [NotMapped]
        [DisplayName("Value")]
        public string? FilterValue { get; set; }
        [NotMapped]
        [DisplayName("Ordening")]
        public Query? Query { get; set; }

        public Receipt()
        {

        }
    }
}
