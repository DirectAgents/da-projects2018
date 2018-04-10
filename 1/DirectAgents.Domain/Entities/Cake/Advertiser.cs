using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.Cake
{
    public class Advertiser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AdvertiserId { get; set; }
        public string AdvertiserName { get; set; }

        public Nullable<int> AccountManagerId { get; set; }
        [ForeignKey("AccountManagerId")]
        public virtual Contact AccountManager { get; set; }

        public Nullable<int> AdManagerId { get; set; }
        [ForeignKey("AdManagerId")]
        public virtual Contact AdManager { get; set; }

        public virtual List<Offer> Offers { get; set; }

        public string AdvertiserNameTrimmed(int maxChars)
        {
            if (AdvertiserName == null)
                return null;
            if (AdvertiserName.Length <= maxChars)
                return AdvertiserName;
            else
                return AdvertiserName.Substring(0, maxChars) + "...";
        }

        public int? ABClientId { get; set; }

        [NotMapped]
        public IEnumerable<int> OfferIds { get; set; }
        [NotMapped]
        public StatsSummary Stats { get; set; }
    }
}
