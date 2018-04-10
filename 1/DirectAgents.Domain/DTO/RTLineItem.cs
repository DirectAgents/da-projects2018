using System.Collections.Generic;
using System.Linq;

namespace DirectAgents.Domain.DTO
{
    // A DTO for standardizing lineItems from the RevTrack systems

    public interface IRTLineItem
    {
        //For the client/advertiser or whoever this lineItem applies to
        int? ABId { get; }
        int RTId { get; } // the Id in the RevTrack system (e.g. Cake)
        string Name { get; }

        decimal Revenue { get; }
        decimal Cost { get; }
        string RevCurr { get; }
        string CostCurr { get; }

        decimal? Units { get; }
        decimal? RevPerUnit { get; }
    }

    public class RTLineItem : IRTLineItem
    {
        public RTLineItem() { }
        public RTLineItem(IEnumerable<IRTLineItem> lineItems)
        {
            Revenue = lineItems.Sum(li => li.Revenue);
            Cost = lineItems.Sum(li => li.Cost);
            // RevCurr, CostCurr, Units, RevPerUnit ?
        }
        public RTLineItem(ProgClientStats pcStats)
        {
            ABId = pcStats.ProgClient.ABClientId; // ?
            RTId = pcStats.ProgClient.Id; // ?
            Name = pcStats.ProgClient.Name; // ?
            Revenue = pcStats.TotalRevenue;
            Cost = pcStats.DACost;
            // RevCurr, CostCurr, Units, RevPerUnit ?
        }
        public RTLineItem(ITDLineItem tdLineItem) //TODO: specify where to take Name from
        {
            if (tdLineItem.ProgVendor != null)
                Name = tdLineItem.ProgVendor.Name;
            Revenue = tdLineItem.TotalRevenue;
            Cost = tdLineItem.DACost;
            // RevCurr, CostCurr, Units, RevPerUnit ?
        }

        public int? ABId { get; set; }
        public int RTId { get; set; }
        public string Name { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public string RevCurr { get; set; }
        public string CostCurr { get; set; }
        public decimal? Units { get; set; }
        public decimal? RevPerUnit { get; set; }

        public IEnumerable<string> CostCurrFromIEnumerable
        {
            set { this.CostCurr = string.Join("/", value); }
        }
    }
}
