using System.Linq;
using System.Threading.Tasks;
using CakeExtracter.CakeMarketingApi;
using CakeExtracter.Common;
using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class TrafficExtracter : Extracter<DateRangeTraffic>
    {
        private readonly DateRange dateRange;

        public TrafficExtracter(DateRange dateRange)
        {
            this.dateRange = dateRange;
        }

        protected override void Extract()
        {
            Logger.Info("Getting traffic for {0}", dateRange);

            var traffic = CakeMarketingUtility.Traffic(dateRange);

            Logger.Info("Extracted {0} Traffic items", traffic.Count);

            Add(new [] { new DateRangeTraffic(dateRange, traffic) }.ToList());

            End();
        }
    }
}
