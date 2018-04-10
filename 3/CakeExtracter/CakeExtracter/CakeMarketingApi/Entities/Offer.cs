using System;
using System.Collections.Generic;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class Offer
    {
        public int OfferId { get; set; }
        public string OfferName { get; set; }
        public Advertiser Advertiser { get; set; }
        public Vertical Vertical { get; set; }
        public OfferType OfferType { get; set; }
        public OfferStatus OfferStatus { get; set; }
        public bool Hidden { get; set; }
        public int DefaultOfferContractId { get; set; }
        public List<OfferContractInfo> OfferContracts { get; set; }
        public Currency Currency { get; set; }
        public DateTime DateCreated { get; set; }
    }
}