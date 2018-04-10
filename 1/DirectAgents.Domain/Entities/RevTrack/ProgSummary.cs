using System;

namespace DirectAgents.Domain.Entities.RevTrack
{
    public class ProgSummary
    {
        public DateTime Date { get; set; }
        public int ProgCampaignId { get; set; }
        public virtual ProgCampaign ProgCampaign { get; set; }
        public int ProgVendorId { get; set; }
        public virtual ProgVendor ProgVendor { get; set; }

        public decimal Cost { get; set; }
        // conversions? (postclick & postview?)
    }

    public class ProgExtraItem
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int ProgCampaignId { get; set; }
        public virtual ProgCampaign ProgCampaign { get; set; }
        public int ProgVendorId { get; set; }
        public virtual ProgVendor ProgVendor { get; set; }

        public string Description { get; set; }
        public decimal Cost { get; set; }
        public decimal Revenue { get; set; }
    }
}
