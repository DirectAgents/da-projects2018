using System.Collections.Generic;

namespace DirectAgents.Domain.Entities.AB
{
    public class ProtoCampaign
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public int ClientId { get; set; }
        //public virtual ABClient Client { get; set; }
        public int ClientAccountId { get; set; }
        public virtual ClientAccount ClientAccount { get; set; }

        public virtual ICollection<ProtoSpendBit> ProtoSpendBits { get; set; }
    }
}
