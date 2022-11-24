using System.ComponentModel;

namespace Expenses_Manager.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        [DisplayName("Nome")]
        public string Name { get; set; }

        public Category()
        {

        }
    }
}
