using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.Cake
{
    public class Camp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CampaignId { get; set; }

        public int AffiliateId { get; set; }
        public virtual Affiliate Affiliate { get; set; }
        public int OfferId { get; set; }
        public virtual Offer Offer { get; set; }
        public int OfferContractId { get; set; }

        public decimal PayoutAmount { get; set; }
        public string CurrencyAbbr { get; set; }

    }
}
