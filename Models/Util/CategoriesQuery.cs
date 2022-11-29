using Expenses_Manager.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace Expenses_Manager.Models.Util
{
    public class CategoriesQuery
    {
        [NotMapped]
        public List<Category>? Categories { get; set; }
        [NotMapped]
        [DisplayName("Ordernar por")]
        public CategoryOrderType CategoriesOrderType { get; set; } = CategoryOrderType.OrdemAlfabetica;
        [NotMapped]
        [DisplayName("Filtrar por")]
        public CategoryFilterType CategoriesFilterType { get; set; } = CategoryFilterType.Nada;
        [NotMapped]
        [DisplayName("Valor do filtro")]
        public string? CategoriesFilterValue { get; set; }
        [NotMapped]
        [DisplayName("Em ordem")]
        public ResultsOrder? CategoriesOrder { get; set; }
    }
}
