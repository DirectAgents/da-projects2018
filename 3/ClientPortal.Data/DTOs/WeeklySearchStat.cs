using System;

namespace ClientPortal.Data.DTOs
{
    public class WeeklySearchStat
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Channel { get; set; }
        public string Campaign { get; set; }
        public int ROAS { get; set; }
        public decimal CPL { get; set; }
    }
}
