using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.Cake
{
    public class Affiliate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AffiliateId { get; set; }
        public string AffiliateName { get; set; }
    }
}
