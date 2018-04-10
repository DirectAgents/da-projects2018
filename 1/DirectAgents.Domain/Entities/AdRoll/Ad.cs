using System;

namespace DirectAgents.Domain.Entities.AdRoll
{
    public class Ad
    {
        public int Id { get; set; }
        public int AdvertisableId { get; set; }
        public virtual Advertisable Advertisable { get; set; }

        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string Eid { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
