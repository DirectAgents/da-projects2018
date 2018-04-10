namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class ContactInfo
    {
        public int ContactId { get; set; }
        public Role Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Title { get; set; }
        public string PhoneWork { get; set; }
        public string PhoneCell { get; set; }
        public string PhoneFax { get; set; }
    }
}
