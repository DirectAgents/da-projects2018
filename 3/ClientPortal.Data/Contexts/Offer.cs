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
    
    public partial class Offer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Offer()
        {
            this.Goals = new HashSet<Goal>();
            this.Campaigns = new HashSet<Campaign>();
            this.Creatives = new HashSet<Creative>();
            this.CPMReports = new HashSet<CPMReport>();
        }
    
        public int OfferId { get; set; }
        public string OfferName { get; set; }
        public Nullable<int> AdvertiserId { get; set; }
        public string DefaultPriceFormat { get; set; }
        public string Currency { get; set; }
        public byte[] Logo { get; set; }
        public Nullable<System.DateTime> LastSynch_Campaigns { get; set; }
        public Nullable<System.DateTime> LastSynch_Creatives { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Goal> Goals { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Campaign> Campaigns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Creative> Creatives { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CPMReport> CPMReports { get; set; }
    }
}