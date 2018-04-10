using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.AB
{
    public class SpendBucket
    {
        public int Id { get; set; }
        public int AcctId { get; set; }
        [ForeignKey("AcctId")]
        public virtual ClientAccount ClientAccount { get; set; }

        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }

        public ICollection<SpendBit> Bits { get; set; }
    }

    public class SpendBit
    {
        public int Id { get; set; }
        public int PeriodId { get; set; }
        public virtual Period Period { get; set; }
        public int SpendBucketId { get; set; }
        public virtual SpendBucket SpendBucket { get; set; }
        public int VendorId { get; set; }
        public virtual ABVendor Vendor { get; set; }
        public int UnitTypeId { get; set; }
        public virtual UnitType UnitType { get; set; }

        public decimal Revenue { get; set; }

        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Cost { get; set; }
        public bool IsFee { get; set; }
    }
}
