using System;
using System.Collections.Generic;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class Creative
    {
        public int CreativeId { get; set; }
        public string CreativeName { get; set; }
        public CreativeType CreativeType { get; set; }
        public DateTime DateCreated { get; set; }
        public int CreativeStatusId { get; set; }
        public string OfferLinkOverride { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public List<CreativeFile> CreativeFiles { get; set; }
    }
}
