namespace DirectAgents.Domain.Entities.CPSearch
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CallDailySummary")]
    public partial class CallDailySummary
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SearchCampaignId { get; set; }

        [Key]
        [Column(Order = 1, TypeName = "date")]
        public DateTime Date { get; set; }

        public int Calls { get; set; }

        public virtual SearchCampaign SearchCampaign { get; set; }
    }
}
