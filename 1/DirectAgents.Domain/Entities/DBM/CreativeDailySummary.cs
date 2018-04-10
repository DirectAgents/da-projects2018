using System;

namespace DirectAgents.Domain.Entities.DBM
{
    public class CreativeDailySummary
    {
        public DateTime Date { get; set; }
        public int InsertionOrderID { get; set; }
        public virtual InsertionOrder InsertionOrder { get; set; }
        public int CreativeID { get; set; }
        public virtual Creative Creative { get; set; }

        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int PostClickConv { get; set; }
        public int PostViewConv { get; set; }
        public decimal Revenue { get; set; }
    }
}
