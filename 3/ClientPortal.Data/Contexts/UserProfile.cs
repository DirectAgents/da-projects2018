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
    
    public partial class UserProfile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserProfile()
        {
            this.UserEvents = new HashSet<UserEvent>();
        }
    
        public int UserId { get; set; }
        public string UserName { get; set; }
        public Nullable<int> CakeAdvertiserId { get; set; }
        public Nullable<int> QuickBooksCompanyId { get; set; }
        public Nullable<int> QuickBooksAdvertiserId { get; set; }
        public Nullable<int> TradingDeskAccountId { get; set; }
        public Nullable<int> SearchProfileId { get; set; }
        public Nullable<int> TDAdvertiserId { get; set; }
        public Nullable<int> ClientInfoId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserEvent> UserEvents { get; set; }
        public virtual SearchProfile SearchProfile { get; set; }
        public virtual ClientInfo ClientInfo { get; set; }
    }
}