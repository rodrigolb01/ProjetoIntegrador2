using Expenses_Manager.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace Expenses_Manager.Models.Util
{
    public class PaymentMethodsQuery
    {
        [NotMapped]
        public List<PaymentMethod>? PaymentMethods { get; set; }
        [NotMapped]
        [DisplayName("Ordernar por")]
        public PaymentMethodOrderType PaymentMethodsOrderType { get; set; } = PaymentMethodOrderType.ValorAtual;
        [NotMapped]
        [DisplayName("Filtrar por")]
        public PaymentMethodFilterType PaymentMethodsFilterType { get; set; } = PaymentMethodFilterType.Nada;
        [NotMapped]
        [DisplayName("Valor do filtro")]
        public string? PaymentMethodsFilterValue { get; set; }
        [NotMapped]
        [DisplayName("Em ordem")]
        public ResultsOrder? PaymentMethodsOrder { get; set; }
    }
}
