using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.Cake
{
    public class CampSum
    {
        public int CampId { get; set; }
        [ForeignKey("CampId")]
        public virtual Camp Camp { get; set; }
        public DateTime Date { get; set; }

        //NOTE: FKs set on Camp; not needed here?
        public int OfferId { get; set; } //TODO: index/FK
        public int AffId { get; set; }   //TODO: index/FK

        public int Views { get; set; }
        public int Clicks { get; set; }
        public decimal Conversions { get; set; }
        public decimal Paid { get; set; }
        public decimal Sellable { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }

        public int RevCurrId { get; set; }
        public virtual Currency RevCurr { get; set; }
        public int CostCurrId { get; set; }
        public virtual Currency CostCurr { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal RevenuePerUnit { get; private set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal CostPerUnit { get; private set; }

        public decimal Units { get; set; }
        public string PriceFormat { get; set; }
    }
}
