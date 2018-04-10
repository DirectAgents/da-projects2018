
namespace DirectAgents.Domain.Entities.AB
{
    public class ProtoSpendBit // could become an "Item" and include Cost
    {
        public int Id { get; set; }
        public int ProtoPeriodId { get; set; }
        public virtual ProtoPeriod ProtoPeriod { get; set; }
        public int ProtoCampaignId { get; set; }
        public virtual ProtoCampaign ProtoCampaign { get; set; }

        // Quantity/NumUnits ? + calculate unitprice or total rev..
        public decimal Revenue { get; set; }
        public string Desc { get; set; }
    }
}
