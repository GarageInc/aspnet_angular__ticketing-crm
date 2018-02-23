namespace LSports.Framework.Models.CustomClasses
{
    public class CustomerContactModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EMail { get; set; }
        public string Skype { get; set; }
        public string Phones { get; set; }
        public int Priority { get; set; }
        public string UserId { get; set; }
        public string Name => $"{FirstName ?? string.Empty} {LastName ?? string.Empty}";
    }
}
