using System;

namespace DirectAgents.Domain.Entities.AdRoll
{
    public class Advertisable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Eid { get; set; }
        public bool Active { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
