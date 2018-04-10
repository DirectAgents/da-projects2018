using System;
using System.Collections.Generic;
using DirectAgents.Domain.Entities.Screen;

namespace DirectAgents.Web.Models
{
    public class SalespeopleStatsVM
    {
        public List<SalespersonStat> Stats { get; set; }
        public DateTime Date { get; set; }
    }
}