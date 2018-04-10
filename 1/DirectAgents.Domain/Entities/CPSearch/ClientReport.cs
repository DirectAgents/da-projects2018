using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.CPSearch
{
    [Table("ClientReport")]
    public partial class ClientReport
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StartDayOfWeek { get; set; }

        public int? SearchProfileId { get; set; }
        public int? ProgCampaignId { get; set; }
    }
}
