namespace Expenses_Manager.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string? HolderName { get; set; }
        public string? Flag { get; set; }
        public bool IsCredit { get; set; }
        public DateTime ExpirationDate { get; set; }
        public double? LimitValue { get; set; }
        public double CurrentValue { get; set; }

        public Card()
        {

        }
    }
}
