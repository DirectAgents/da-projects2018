using System;
using System.Collections.Generic;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Web.Areas.ProgAdmin.Models
{
    public class DailySummariesVM
    {
        public ExtAccount ExtAccount { get; set; }
        public DateTime? Month { get; set; }
        public IEnumerable<DailySummary> DailySummaries { get; set; }
    }
}