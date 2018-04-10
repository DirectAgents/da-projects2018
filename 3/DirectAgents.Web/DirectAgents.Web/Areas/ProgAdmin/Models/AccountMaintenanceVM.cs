using System;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Web.Areas.ProgAdmin.Models
{
    public class AccountMaintenanceVM
    {
        public ExtAccount ExtAccount { get; set; }
        public bool Syncable { get; set; }

        public TDStatsGauge StatsGauge { get; set; }
    }
}