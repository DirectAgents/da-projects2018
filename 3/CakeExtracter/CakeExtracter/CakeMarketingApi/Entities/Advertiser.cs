using System.Collections.Generic;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class Advertiser
    {
        public int AdvertiserId { get; set; }
        public string AdvertiserName { get; set; }
        public List<Contact> AccountManagers { get; set; }
        public List<ContactInfo> Contacts { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
