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
    
    public partial class Affiliate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Affiliate()
        {
            this.Campaigns = new HashSet<Campaign>();
        }
    
        public int AffiliateId { get; set; }
        public string AffiliateName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Campaign> Campaigns { get; set; }
    }
}
