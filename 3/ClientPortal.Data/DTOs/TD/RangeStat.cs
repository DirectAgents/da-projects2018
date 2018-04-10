using System;

//note: this is modeled after SearchStat

namespace ClientPortal.Data.DTOs.TD
{
    public class RangeStat : StatsSummaryBase
    {
        public string Title { get; set; }
        public string Range { get; set; }
        public int Days { get; set; }

        public DateTime StartDate
        {
            get { return EndDate.AddDays((Days - 1) * -1); }
        }

        private DayOfWeek WeekEndDay
        {
            get
            {
                if (this.WeekStartDay == DayOfWeek.Sunday)
                    return DayOfWeek.Saturday;
                else
                    return this.WeekStartDay - 1;
            }
        }

        // --- Pre-initializers ---

        // Set this first - unless using default value
        internal DayOfWeek WeekStartDay
        {
            set { _weekStartDay = value; }
            get { return _weekStartDay; }
        }
        private DayOfWeek _weekStartDay = DayOfWeek.Monday;

        // Set this next, if required by initializer
        public DateTime EndDate { get; set; }

        // Optionally, set this to "latest" (yesterday or today) - to fill in holes in dateranges
        internal DateTime? FillToLatest { get; set; } // TODO: implement for other than WeekByMaxDate

        // Then use one of the following...

        // --- Initializers (monthly, weekly or custom date range) ---

        internal DateTime MonthByMaxDate
        {
            set
            {
                this.EndDate = value;
                this.Days = this.EndDate.Day;

                this.Range = ToRangeName(this.EndDate, false, true);
                this.Title = ToRangeName(this.EndDate, false);
            }
        }

        internal DateTime WeekByMaxDate
        {
            set
            {
                var startDate = value;
                // Stretch back until we reach a WeekStartDay
                while (startDate.DayOfWeek != this.WeekStartDay)
                    startDate = startDate.AddDays(-1);

                this.EndDate = value;
                if (this.FillToLatest.HasValue)
                {
                    // Stretch forward until we reach a WeekEndDay (or FillToLatest, whichever comes first)
                    var weekEndDay = this.WeekEndDay;
                    while (this.EndDate < this.FillToLatest.Value && this.EndDate.DayOfWeek != weekEndDay)
                        this.EndDate = this.EndDate.AddDays(1);
                }
                this.Days = (this.EndDate - startDate).Days + 1;

                this.Range = ToRangeName(startDate, this.EndDate, true);
                this.Title = ToRangeName(startDate, this.EndDate);
            }
        }

        internal DateTime CustomByStartDate // (note: set (end) Date first)
        {
            set
            {
                if (value > this.EndDate)
                    throw new Exception("Must set (end) Date first and start date must be <= end date.");

                this.Days = (this.EndDate - value).Days + 1;
                this.Range = ToRangeName(value, this.EndDate);
            }
        }

        // will set Title only if value is not null
        //internal string TitleIfNotNull
        //{
        //    set { if (value != null) this.Title = value; }
        //}

        // --- Constructors ---

        // call this when using one of the above initializers
        public RangeStat() { }

        // --- Private methods ---

        private string ToRangeName(DateTime date, bool isWeekly, bool prependYMD = false)
        {
            if (isWeekly)
            {
                DateTime weekStart = date.AddDays(-6);
                return ToRangeName(weekStart, date, prependYMD);
            }
            else
                return (prependYMD ? date.ToString("yyyyMMdd") + " " : "") + date.ToString("MMM-yy");
        }

        private string ToRangeName(DateTime start, DateTime end, bool prependYMD = false)
        {
            return (prependYMD ? start.ToString("yyyyMMdd") + " " : "") + start.ToString("MM/dd") + " - " + end.ToString("MM/dd");
        }
    }
}
