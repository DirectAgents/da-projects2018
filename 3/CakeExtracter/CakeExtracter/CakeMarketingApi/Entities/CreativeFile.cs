using System;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class CreativeFile
    {
        public int CreativeFileId { get; set; }
        public string CreativeFileName { get; set; }
        public string CreativeFileLink { get; set; }
        public bool Preview { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
