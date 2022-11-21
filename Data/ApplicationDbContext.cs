using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Expenses_Manager.Models;

namespace Expenses_Manager.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Expenses_Manager.Models.UserData> UserData { get; set; }
        public DbSet<Expenses_Manager.Models.Expense> Expense { get; set; }
        public DbSet<Expenses_Manager.Models.Receipt> Receipt { get; set; }
        public DbSet<Expenses_Manager.Models.PaymentMethod> PaymentMethod { get; set; }
        public DbSet<Expenses_Manager.Models.Category> Category { get; set; }

        public int currentReceiptId { get; set; }
    }
}