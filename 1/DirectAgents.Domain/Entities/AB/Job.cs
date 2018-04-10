using System.Collections.Generic;

namespace DirectAgents.Domain.Entities.AB
{
    public class Job
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public virtual ABClient Client { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ABExtraItem> ExtraItems { get; set; }
    }
}
