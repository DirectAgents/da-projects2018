namespace DirectAgents.Domain.Entities.CPSearch
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SearchConvSummary")]
    public partial class SearchConvSummary
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SearchCampaignId { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime Date { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SearchConvTypeId { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(1)]
        public string Network { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(1)]
        public string Device { get; set; }

        public double Conversions { get; set; }

        public decimal ConVal { get; set; }

        public virtual SearchCampaign SearchCampaign { get; set; }

        public virtual SearchConvType SearchConvType { get; set; }
    }
}
