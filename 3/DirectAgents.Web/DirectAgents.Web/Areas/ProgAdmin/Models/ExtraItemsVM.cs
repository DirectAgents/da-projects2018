using System;
using System.Collections.Generic;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Web.Areas.ProgAdmin.Models
{
    public class ExtraItemsVM
    {
        public Campaign Campaign { get; set; }
        public int? CampaignId
        {
            get { return (Campaign != null ? Campaign.Id : (int?)null); }
        }
        public DateTime? Month { get; set; }
        public IEnumerable<ExtraItem> Items { get; set; }
    }
}