using System;

namespace DirectAgents.Domain.Entities.CPProg
{
    public class BasicStat
    {
        public int Day { get; set; }
        public DateTime Date { get; set; } // also used for End Date
        public DateTime StartDate { get; set; } // also used for Start Day of Week/Month

        public string DayName
        {
            get { return Enum.GetName(typeof(DayOfWeek), Day - 1); }
        }

        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }

        public decimal MediaSpend { get; set; }
        public decimal MgmtFee { get; set; }
        public double Budget { get; set; }
        public double Pacing { get; set; }

        public double CTR { get; set; }
        public double CR { get; set; }
        public double eCPC { get; set; }
        public double eCPA { get; set; }

        public int AccountId { get; set; }

        // For Strategy/Creative Stats...
        public string PlatformAlias { get; set; }
        public int StrategyId { get; set; }
        public string StrategyName { get; set; }
        public int PostClickConv { get; set; }
        public int PostViewConv { get; set; }
        public bool ShowClickAndViewConv { get; set; }
        public int AdId { get; set; }
        public string AdName { get; set; }
        public string Url { get; set; }
        public int AdWidth { get; set; }
        public int AdHeight { get; set; }
        public string AdBody { get; set; }
        public string AdHeadline { get; set; }
        public string AdMessage { get; set; }
        public string AdDestinationUrl { get; set; }

        //public double SumKPI { get; set; }

        // For Site Stats...
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        public void ComputeCalculatedStats()
        {
            CTR = (Impressions == 0) ? 0 : (double)Clicks / Impressions;
            CR = (Clicks == 0) ? 0 : (double)Conversions / Clicks;
            eCPC = (Clicks == 0) ? 0 : (double)(MediaSpend / Clicks);
            eCPA = (Conversions == 0) ? 0 : (double)(MediaSpend / Conversions);
            Pacing = (Budget == 0) ? 0 : (double)MediaSpend / Budget;
        }
        //TODO: rounding - 4 decimals?

        public void ComputeWeekStartDate(DayOfWeek startDayOfWeek = DayOfWeek.Monday)
        {
            StartDate = Date;
            while (StartDate.DayOfWeek != startDayOfWeek)
                StartDate = StartDate.AddDays(-1);
        }
        public void ComputeMonthStartDate()
        {
            StartDate = new DateTime(Date.Year, Date.Month, 1);
        }
    }

    public class LeadInfo
    {
        public DateTime Time { get; set; }
        public string ConvType { get; set; }
        public decimal ConvVal { get; set; }
        public string LeadID { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string IP { get; set; }

        public string StrategyName { get; set; }
        public int? StrategyId { get; set; }
        public string AdName { get; set; }
        public int? AdId { get; set; }
    }
}
