using System;
using System.Collections.Generic;
using System.Linq;

namespace ClientPortal.Data.DTOs
{
    public class DailyInfo
    {
        public string Id { get { return Date.ToString("yyyyMMdd"); } }

        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set { _date = new DateTime(value.Ticks, DateTimeKind.Utc); }
        }

        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }
        public float ConversionPct
        {
            get { return (Clicks == 0) ? 0 : (float)Math.Round((double)Conversions / Clicks, 3); }
        }
        public decimal Revenue { get; set; }
        public decimal EPC
        {
            get { return (Clicks == 0) ? 0 : Math.Round(Revenue / Clicks, 2); }
        }
        public string Currency
        {
            set { Culture = OfferInfo.CurrencyToCulture(value); }
        }
        public string Culture { get; set; }


        public static IQueryable<DailyInfo> MakeCumulative(IQueryable<DailyInfo> dailyInfos)
        {
            var runningTotals = new DailyInfo();
            dailyInfos = dailyInfos.OrderBy(di => di.Date).AsEnumerable().Select(di => new DailyInfo
            {
                Date = di.Date,
                Impressions = runningTotals.Impressions += di.Impressions,
                Clicks = runningTotals.Clicks += di.Clicks,
                Conversions = runningTotals.Conversions += di.Conversions,
                Revenue = runningTotals.Revenue += di.Revenue,
                Culture = di.Culture
            })
            .ToList().AsQueryable();
            return dailyInfos;
        }

        // project cumulative infos to the end of the month...
        public static IQueryable<DailyInfo> AddProjection(IQueryable<DailyInfo> dailyInfos, out DailyInfo projectionInfo)
        {
            int numInfos = dailyInfos.Count();
            if (numInfos == 0)
            {
                projectionInfo = null;
                return dailyInfos;
            }
            var orderedInfos = dailyInfos.OrderBy(di => di.Date);
            var firstInfo = orderedInfos.First();
            var firstDate = firstInfo.Date;

            var reverseOrderedInfos = dailyInfos.OrderByDescending(di => di.Date);
            var savedLastInfo = reverseOrderedInfos.First(); // see below
            var lastInfo = savedLastInfo;

            if (lastInfo.Date == DateTime.Today)
            {
                if (numInfos == 1)
                { // Don't project because all we have is today and that's partial data.
                    projectionInfo = null;
                    return dailyInfos;
                }
                // We have more than one dailyInfo; don't include today's stats in the calculations.
                lastInfo = reverseOrderedInfos.Skip(1).First();
            }
            var lastDate = lastInfo.Date;

            //            if (firstDate.Year != lastDate.Year || firstDate.Month != lastDate.Month) return dailyInfos; // if multiple months

            var daysInMonth = DateTime.DaysInMonth(lastDate.Year, lastDate.Month);
            if (lastDate.Day == daysInMonth) // if last day of the month, nothing to project
            { // Since we're not including "today", this would only happen if we were looking at a previous month for some reason
                projectionInfo = lastInfo;
                return dailyInfos;
            }

            var projectionDate = new DateTime(lastDate.Year, lastDate.Month, daysInMonth);
            var numOverallDays = (projectionDate - firstDate).Days + 1;

            var numDays = (lastDate - firstDate).Days + 1; // # of days in the supplied range

            projectionInfo = new DailyInfo()
            {
                Date = projectionDate,
                Impressions = numOverallDays * lastInfo.Impressions / numDays,
                Clicks = numOverallDays * lastInfo.Clicks / numDays,
                Conversions = numOverallDays * lastInfo.Conversions / numDays,
                Revenue = numOverallDays * lastInfo.Revenue / numDays,
                Culture = lastInfo.Culture
            };
            if (projectionDate != savedLastInfo.Date) // (Don't add it if the date is already there)
                dailyInfos = dailyInfos.Concat(new List<DailyInfo>() { projectionInfo });

            return dailyInfos;
        }

    }
}
