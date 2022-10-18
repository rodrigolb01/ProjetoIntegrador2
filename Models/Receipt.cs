using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expenses_Manager.Models
{
    public class Receipt
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public double TotalValue { get; set; }
        public bool PendingPayments { get; set; }
        [NotMapped]
        public SelectList? Expenses { get; set; }

        public Receipt()
        {

        }
    }
}
