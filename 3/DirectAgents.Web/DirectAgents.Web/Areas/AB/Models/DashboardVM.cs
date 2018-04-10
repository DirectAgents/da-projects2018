using System;
using System.Collections.Generic;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Web.Areas.AB.Models
{
    public class DashboardVM
    {
        public ABClient ABClient { get; set; }

        // Use one or the other...
        public IEnumerable<ABStat> ABStats { get; set; }
        public IEnumerable<ABLineItem> LineItems { get; set; }
    }

    //TODO: what should this be called? (This is the main view for a single client.)
    public class DetailVM
    {
        public ABClient ABClient { get; set; }
        public IEnumerable<MonthGroup> MonthGroups { get; set; }
    }
    public class MonthGroup
    {
        public DateTime Month { get; set; }
        public IEnumerable<ABLineItem> LineItems { get; set; }
        public IEnumerable<JobGroup> JobGroups { get; set; }
    }
    public class JobGroup
    {
        public Job Job { get; set; }
        public IEnumerable<ABLineItem> LineItems { get; set; }
    }

    // for Single Client Detail view
    public class ProtoVM
    {
        public ABClient ABClient { get; set; }
        public IEnumerable<PeriodGroup> PeriodGroups { get; set; }
    }
    public class PeriodGroup
    {
        public DateTime Month { get; set; } // ProtoPeriod?
        public IEnumerable<AccountGroup> AccountGroups { get; set; }
    }
    public class AccountGroup
    {
        public string Name { get; set; }
        public IEnumerable<ABLineItem> LineItems { get; set; }
    }
}