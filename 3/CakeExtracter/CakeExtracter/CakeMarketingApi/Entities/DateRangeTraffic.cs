using System.Collections.Generic;
using CakeExtracter.Common;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class DateRangeTraffic
    {
        public DateRangeTraffic()
        {
        }

        public DateRangeTraffic(DateRange dr, List<Traffic> t)
        {
            DateRange = dr;
            Traffic = t;
        }

        public DateRange DateRange { get; set; }

        public List<Traffic> Traffic { get; set; }
    }
}
