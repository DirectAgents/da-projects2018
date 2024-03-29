//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ClientPortal.Data.Contexts
{
    using System;
    using System.Collections.Generic;
    
    public partial class Campaign
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Campaign()
        {
            this.CampaignDrops = new HashSet<CampaignDrop>();
        }
    
        public int CampaignId { get; set; }
        public int OfferId { get; set; }
        public int AffiliateId { get; set; }
        public string CampaignName { get; set; }
        public string PriceFormatName { get; set; }
    
        public virtual Affiliate Affiliate { get; set; }
        public virtual Offer Offer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CampaignDrop> CampaignDrops { get; set; }
    }
}
