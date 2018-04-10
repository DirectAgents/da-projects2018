namespace DirectAgents.Domain.Entities.CPSearch
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SearchAccount")]
    public partial class SearchAccount
    {
        public const string GoogleChannel = "Google";
        public const string BingChannel = "Bing";
        public const string AppleChannel = "Apple";
        //public const string CriteoChannel = "Criteo";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SearchAccount()
        {
            SearchCampaigns = new HashSet<SearchCampaign>();
            AltSearchCampaigns = new HashSet<SearchCampaign>();
        }

        public int SearchAccountId { get; set; }

        public int? AdvertiserId { get; set; }

        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Channel { get; set; }

        [StringLength(50)]
        public string AccountCode { get; set; }

        [StringLength(50)]
        public string ExternalId { get; set; }

        public int? SearchProfileId { get; set; }

        public decimal? RevPerOrder { get; set; }

        [Column(TypeName = "date")]
        public DateTime? MinSynchDate { get; set; }

        public virtual SearchProfile SearchProfile { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SearchCampaign> SearchCampaigns { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SearchCampaign> AltSearchCampaigns { get; set; }

        [NotMapped]
        public string DisplayName1 { get { return "[" + SearchAccountId + "] " + Name; } }
        [NotMapped]
        public DateTime? MinDaySum { get; set; }
        [NotMapped]
        public DateTime? MaxDaySum { get; set; }
        [NotMapped]
        public DateTime? MinConvSum { get; set; }
        [NotMapped]
        public DateTime? MaxConvSum { get; set; }
        [NotMapped]
        public DateTime? MinCallSum { get; set; }
        [NotMapped]
        public DateTime? MaxCallSum { get; set; }
    }
}
