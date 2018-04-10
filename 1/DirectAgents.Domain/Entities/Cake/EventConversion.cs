using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.Cake
{
    public class Event
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PriceFormat
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class EventConversion
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public DateTime ConvDate { get; set; }
        public DateTime ClickDate { get; set; }

        public int EventId { get; set; }
        public virtual Event Event { get; set; }
        public int AffiliateId { get; set; }
        public virtual Affiliate Affiliate { get; set; }
        public int OfferId { get; set; }
        public virtual Offer Offer { get; set; }

        public string SubId1 { get; set; }
        public string SubId2 { get; set; }
        public string SubId3 { get; set; }
        public string SubId4 { get; set; }
        public string SubId5 { get; set; }

        public int PriceFormatId { get; set; }
        public virtual PriceFormat PriceFormat { get; set; }

        public decimal Paid { get; set; }
        public decimal Received { get; set; }
        public int PaidCurrId { get; set; }
        public virtual Currency PaidCurr { get; set; }
        public int ReceivedCurrId { get; set; }
        public virtual Currency ReceivedCurr { get; set; }
    }
}
