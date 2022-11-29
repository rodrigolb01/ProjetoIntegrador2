using Expenses_Manager.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace Expenses_Manager.Models.Util
{
    public class ReceiptsQuery
    {
        [NotMapped]
        public List<Receipt>? Receipts { get; set; }

        [NotMapped]
        [DisplayName("Ordernar por")]
        public ReceiptOrderType ReceiptsOrderType { get; set; } = ReceiptOrderType.Mes;
        [NotMapped]
        [DisplayName("Filtrar por")]
        public ReceiptFilterType ReceiptsFilterType { get; set; } = ReceiptFilterType.Nada;
        [NotMapped]
        [DisplayName("Valor do filtro")]
        public string? ReceiptsFilterValue { get; set; }
        [NotMapped]
        [DisplayName("Em ordem")]
        public ResultsOrder? ReceiptsOrder { get; set; }

    }
}
