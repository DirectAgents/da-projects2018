using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Contexts
{
    public partial class SimpleReport
    {
        public const int DefaultPeriodMonths = 0;
        public const int DefaultPeriodDays = 7;
        public const int DefaultNumWeeks = 10;
        public const int DefaultNumMonths = 12;

        [NotMapped]
        public bool IncludeAttachment { get; set; }
        [NotMapped]
        public int Attachment_NumWeeks { get; set; }
        [NotMapped]
        public int Attachment_NumMonths { get; set; }
        [NotMapped]
        public string Attachment_Filename { get; set; }

        public string GetDefaultFilename()
        {
            return this.ParentName.Replace(" ", "") + "_" + GetStatsEndDate().ToString("yyyyMMdd") + ".xlsx";
        }

        public void InitializePeriodAndNextSend() // Note: be sure Advertiser or SearchProfile is set
        {
            this.PeriodMonths = DefaultPeriodMonths;
            this.PeriodDays = DefaultPeriodDays;
            this.NextSend = ComputeNextSend_Weekly();
        }
        // used for weekly reports (PeriodDays == 7)
        private DateTime? ComputeNextSend_Weekly(bool includeToday = false)
        {
            if (Advertiser == null && SearchProfile == null)
                return null;

            var nextSend = DateTime.Today.AddYears(10);
            if (Advertiser != null)
            {
                nextSend = Advertiser.GetNext_WeekStartDate(includeToday);
            }
            if (SearchProfile != null)
            {
                var searchNextSend = SearchProfile.GetNext_WeekStartDate(includeToday);
                if (searchNextSend < nextSend)
                    nextSend = searchNextSend;
            }
            return nextSend;
        }

        [NotMapped]
        public bool IsSearchOnly
        {
            get { return (Advertiser == null && SearchProfile != null); }
        }

        [NotMapped]
        public string ParentName
        {
            get
            {
                if (Advertiser != null && SearchProfile == null)
                    return Advertiser.AdvertiserName;
                else if (IsSearchOnly)
                    return SearchProfile.SearchProfileName;
                else if (Advertiser != null && SearchProfile != null)
                    return Advertiser.AdvertiserName + " / " + SearchProfile.SearchProfileName;
                else
                    return null;
            }
        }

        [NotMapped]
        public string DurationString
        {
            get
            {
                if (PeriodMonths > 0)
                    return PeriodMonths + " months";
                if (PeriodDays > 0)
                    return PeriodDays + " days";
                else
                    return null;
            }
        }

        // for the next report that's sent...
        public DateTime GetStatsStartDate()
        {
            DateTime dayAfterEnd = this.NextSend ?? DateTime.Today;
            if (this.PeriodMonths > 0)
                return dayAfterEnd.AddMonths(-1 * this.PeriodMonths);
            else
                return dayAfterEnd.AddDays(-1 * this.PeriodDays);
        }
        public DateTime GetStatsEndDate()
        {
            DateTime dayAfterEnd = this.NextSend ?? DateTime.Today;
            return dayAfterEnd.AddDays(-1);
        }

        public DayOfWeek? GetStartDayOfWeek()
        {
            if (this.Advertiser != null)
                return (DayOfWeek)this.Advertiser.StartDayOfWeek;
            else if (this.SearchProfile != null)
                return (DayOfWeek)this.SearchProfile.StartDayOfWeek;
            else
                return null;
        }

        public void SetEditableFieldsFrom(SimpleReport inReport)
        {
            this.Enabled = inReport.Enabled;
            this.Email = inReport.Email;
            this.EmailCC = inReport.EmailCC;
            this.NextSend = inReport.NextSend;
            this.PeriodMonths = inReport.PeriodMonths;
            this.PeriodDays = inReport.PeriodDays;
        }
    }
}
