namespace DirectAgents.Domain.Entities.CPSearch
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SearchProfile")]
    public partial class SearchProfile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SearchProfile()
        {
            SearchAccounts = new HashSet<SearchAccount>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SearchProfileId { get; set; }

        public string SearchProfileName { get; set; }

        public int StartDayOfWeek { get; set; }

        public bool ShowSearchChannels { get; set; }

        [StringLength(100)]
        public string LCaccid { get; set; }

        public int CallMinSeconds { get; set; }

        public bool ShowRevenue { get; set; }

        public bool UseConvertedClicks { get; set; }

        public bool ShowViewThrus { get; set; }

        public bool ShowCassConvs { get; set; }

        public bool UseAllConvs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SearchAccount> SearchAccounts { get; set; }

        [NotMapped]
        public bool ShowCalls
        {
            get { return !String.IsNullOrWhiteSpace(this.LCaccid); }
        }
    }
}
