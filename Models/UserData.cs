namespace Expenses_Manager.Models
{
    public class UserData
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? MailAddress { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? AddressLine { get; set; }
        public string? ProfilePicture { get; set; }

        public UserData()
        {

        }
    }
}
