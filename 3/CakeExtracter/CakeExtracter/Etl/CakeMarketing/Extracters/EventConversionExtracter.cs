using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class EventConversionExtracter : Extracter<EventConversion>
    {
        private readonly DateRange dateRange;
        private readonly int advertiserId;
        private readonly int offerId;

        public EventConversionExtracter(DateRange dateRange, int advertiserId, int offerId)
        {
            this.dateRange = dateRange;
            this.advertiserId = advertiserId;
            this.offerId = offerId;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting EventConversions from {0:d} to {1:d}, AdvId {2} OffId {3}",
                        dateRange.FromDate, dateRange.ToDate, advertiserId, offerId);
            foreach (var date in dateRange.Dates)
            {
                var singleDate = new DateRange(date, date.AddDays(1));
                var eventConvs = CakeMarketingUtility.EventConversions(singleDate, advertiserId, offerId);
                Add(eventConvs);
            }
            End();
        }
    }
}
