using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.Cake
{
    public class OfferType
    {
        public const int Hosted = 1;
        public const int HostnPost = 2;
        public const int ThirdParty = 3;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OfferTypeId { get; set; }
        public string OfferTypeName { get; set; }
    }
}
