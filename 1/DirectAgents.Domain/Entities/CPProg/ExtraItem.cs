using System;

namespace DirectAgents.Domain.Entities.CPProg
{
    public class ExtraItem
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
        public int PlatformId { get; set; }
        public virtual Platform Platform { get; set; }

        public string Description { get; set; }
        public decimal Cost { get; set; }
        public decimal Revenue { get; set; }
    }
}
