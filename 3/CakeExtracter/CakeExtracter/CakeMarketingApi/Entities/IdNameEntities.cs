namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class AccountManager1
    {
        public int AccountManagerId { get; set; }
        public string AccountManagerName { get; set; }
    }

    public class Advertiser1
    {
        public int AdvertiserId { get; set; }
        public string AdvertiserName { get; set; }
    }

    public class Affiliate1
    {
        public int AffiliateId { get; set; }
        public string AffiliateName { get; set; }
    }

    public class BrandAdvertiser1
    {
        public int BrandAdvertiserId { get; set; }
        public string BrandAdvertiserName { get; set; }
    }

    public class Campaign1
    {
        public int CampaignId { get; set; }
        // CampaignType... Id & Name
    }

    public class Creative1
    {
        public int CreativeId { get; set; }
        public string CreativeName { get; set; }
    }

    public class EventInfo1
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
    }

    public class Isp1
    {
        public int IspId { get; set; }
        public string IspName { get; set; }
    }

    public class Language1
    {
        public int LanguageId { get; set; }
        public string LanguageName { get; set; }
    }

    public class Offer1
    {
        public int OfferId { get; set; }
        public string OfferName { get; set; }
    }

    public class SourceAffiliate1
    {
        public int SourceAffiliateId { get; set; }
        public string SourceAffiliateName { get; set; }
    }

    public class SiteOffer1
    {
        public int SiteOfferId { get; set; }
        public string SiteOfferName { get; set; }
    }

    public class SiteOfferContract1
    {
        public int SiteOfferContractId { get; set; }
        public string SiteOfferContractName { get; set; }
    }

    public class Version1
    {
        public int VersionId { get; set; }
        public string VersionName { get; set; }
    }
}
