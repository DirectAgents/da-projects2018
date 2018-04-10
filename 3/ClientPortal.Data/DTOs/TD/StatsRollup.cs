using System;

namespace ClientPortal.Data.DTOs.TD
{
    public class StatsRollup
    {
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public int NumCreatives { get; set; }
    }
}
