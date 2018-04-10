using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class CreativeMonthlySummariesExtracter : Extracter<CreativeMonthlySummary>
    {
        private readonly DateRange dateRange;
        private readonly int creativeId;

        public CreativeMonthlySummariesExtracter(DateRange dateRange, int creativeId)
        {
            this.dateRange = dateRange;
            this.creativeId = creativeId;
        }

        protected override void Extract()
        {
            var monthlySummaries = ExtractMonthlySummaries().ToList();
            Logger.Info("Extracted {0} MonthlySummaries for creativeId={1}", monthlySummaries.Count, creativeId);
            Add(monthlySummaries);
            End();
        }

        private IEnumerable<CreativeMonthlySummary> ExtractMonthlySummaries()
        {
            Logger.Info("Extracting Summaries for creativeId={0} between {1} and {2}",
                    creativeId,
                    dateRange.FromDate.ToShortDateString(),
                    dateRange.ToDate.ToShortDateString());

            var dailySummaries = CakeMarketingUtility.DailySummaries(dateRange, 0, 0, creativeId, 0);

            DateTime iDate = new DateTime(dateRange.FromDate.Year, dateRange.FromDate.Month, 1);
            while (iDate < dateRange.ToDate)
            {
                var jDate = iDate.AddMonths(1); // next month
                var summaries = dailySummaries.Where(ds => ds.Date >= iDate && ds.Date < jDate);

                var monthSums = new DailySummary
                {
                    Date = iDate,
                    Views = summaries.Sum(s => s.Views),
                    Clicks = summaries.Sum(s => s.Clicks),
                    Conversions = summaries.Sum(s => s.Conversions)
                };

                var monthlySummary = new CreativeMonthlySummary
                {
                    CreativeId = creativeId,
                    Summary = monthSums
                };

                yield return monthlySummary;

                iDate = iDate.AddMonths(1);
            }
        }
    }
}
