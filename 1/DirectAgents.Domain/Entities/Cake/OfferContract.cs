using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.Cake
{
    public class OfferContract
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OfferContractId { get; set; }
        public int OfferId { get; set; }
        public virtual Offer Offer { get; set; }

        //public int? PriceFormatId { get; set; }
        public string PriceFormatName { get; set; }
        //public decimal PayoutAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        //public string OfferLink { get; set; }
        //public string ThankyouLink { get; set; }
        //public bool Hidden { get; set; }
        //TargetingMethod
        //GeoTargeting
    }
}
