using System;
using System.Collections.Generic;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class EventConversionResponse
    {
        public bool Success { get; set; }
        public int RowCount { get; set; }
        public List<EventConversion> EventConversions { get; set; }
    }

    public class EventConversion
    {
        public int EventConversionId { get; set; }
        public int VisitorId { get; set; }
        public int OriginalVisitorId { get; set; }
        public int TrackingId { get; set; }
        public int OriginalTrackingId { get; set; }
        public int RequestSessionId { get; set; }
        public int ClickRequestSessionId { get; set; }
        public int ClickId { get; set; }
        public DateTime EventConversionDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime ClickDate { get; set; }
        public DateTime SourceDate { get; set; }
        public EventInfo1 EventInfo { get; set; }
        public SourceAffiliate1 SourceAffiliate { get; set; }
        public BrandAdvertiser1 BrandAdvertiser { get; set; }
        public SiteOffer1 SiteOffer { get; set; }
        public SiteOfferContract1 SiteOfferContract { get; set; }
        public Campaign1 Campaign { get; set; }
        public Creative1 Creative { get; set; }
        public string SubId1 { get; set; }
        public string SubId2 { get; set; }
        public string SubId3 { get; set; }
        public string SubId4 { get; set; }
        public string SubId5 { get; set; }
        public string EventConversionIpAddress { get; set; }
        public string ClickIpAddress { get; set; }
        public string EventConversionReferrerUrl { get; set; }
        public string ClickReferrerrUrl { get; set; }
        public string EventConversionUserAgent { get; set; }
        public string ClickUserAgent { get; set; }
        public string SourceType { get; set; }
        public PriceFormat PriceFormat { get; set; }
        public MoneyAmount Paid { get; set; }
        //public MoneyAmount PaidUnbilled { get; set; }
        public MoneyAmount Received { get; set; }
        //public MoneyAmount ReceivedUnbilled { get; set; }
        //SiteOfferCreditPercentage
        //SiteOfferPaymentPercentage
        //ProgramCreditPercentage
        public bool PixelDropped { get; set; }
        public bool Suppressed { get; set; }
        public bool Returned { get; set; }
        public bool Test { get; set; }
        public string TransactionId { get; set; }
        //public MoneyAmount OrderTotal { get; set; }
        //EventConversionScore
        public Country Country { get; set; }
        public Region Region { get; set; }
        public Language1 Language { get; set; }
        public Isp1 Isp { get; set; }
        public Device Device { get; set; }
        public OperatingSystem OperatingSystem { get; set; }
        public Browser Browser { get; set; }
        //Note

        public void CopyValuesTo(DirectAgents.Domain.Entities.Cake.EventConversion ec)
        {
            ec.ConvDate = this.EventConversionDate;
            ec.ClickDate = this.ClickDate;
            ec.EventId = this.EventInfo.EventId;
            ec.AffiliateId = this.SourceAffiliate.SourceAffiliateId;
            ec.OfferId = this.SiteOffer.SiteOfferId;
            ec.SubId1 = this.SubId1;
            ec.SubId2 = this.SubId2;
            ec.SubId3 = this.SubId3;
            ec.SubId4 = this.SubId4;
            ec.SubId5 = this.SubId5;
            ec.PriceFormatId = this.PriceFormat.PriceFormatId;
            ec.Paid = this.Paid.Amount;
            ec.Received = this.Received.Amount;
            ec.PaidCurrId = this.Paid.CurrencyId;
            ec.ReceivedCurrId = this.Received.CurrencyId;
        }
    }
}
