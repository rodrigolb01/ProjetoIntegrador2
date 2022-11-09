using Microsoft.AspNetCore.Mvc.Rendering;
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
        public SelectList? Expenses { get; set; }

        public Receipt()
        {

        }
    }
}
