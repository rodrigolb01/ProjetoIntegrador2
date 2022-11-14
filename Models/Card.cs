namespace Expenses_Manager.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string? HolderName { get; set; }
        public string? Number { get; set; } = "0000000000000000";
        public string? SecurityCode { get; set; } = "000";
        public string? Flag { get; set; } = "";
        public bool IsCredit { get; set; } = false;
        public DateTime ExpirationDate { get; set; } = DateTime.Now;
        public double? LimitValue { get; set; } = 0;
        public double CurrentValue { get; set; } = 0;

        public Card()
        {

        }
    }
}
