using System;

namespace DirectAgents.Domain.Entities.AB
{
    public class ABExtraItem
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }

        public int JobId { get; set; }
        public virtual Job Job { get; set; }
    }
}
