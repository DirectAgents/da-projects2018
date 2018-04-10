using System;
using System.Collections.Generic;

namespace CakeExtracter.Common
{
    public struct DateRange
    {
        private readonly Func<DateTime, DateTime> step;

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public DateRange(DateTime fromDate, DateTime toDate, bool datePartOnly = true)
            : this()
        {
            step = x => x.AddDays(1);
            FromDate = datePartOnly ? fromDate.Date : fromDate;
            ToDate = datePartOnly ? toDate.Date : toDate;
        }

        public override string ToString()
        {
            return string.Format("{0} to {1}", FromDate.ToShortDateString(), ToDate.ToShortDateString());
        }

        //public DateRange(DateTime fromDate, DateTime toDate, Func<DateTime, DateTime> step)
        //    : this(fromDate, toDate)
        //{
        //    this.step = step;
        //}

        //public DateRange(int fromYear, int fromMonth, int fromDay, int toYear, int toMonth, int toDay)
        //{
        //    FromDate = new DateTime(fromYear, fromMonth, fromDay).Date;
        //    ToDate = new DateTime(toYear, toMonth, toDay).Date;
        //}

        //public DateRange(DateTime fromDate, int numDays)
        //{
        //    FromDate = fromDate.Date;
        //    ToDate = fromDate.AddDays(numDays).Date;
        //}

        //public DateRange(int fromYear, int fromMonth, int fromDay, Func<DateTime, DateTime> toDate)
        //{
        //    FromDate = new DateTime(fromYear, fromMonth, fromDay).Date;
        //    ToDate = toDate(FromDate).Date;
        //}

        //public IEnumerable<DateRange> DateRanges
        //{
        //    get
        //    {
        //        for (var i = FromDate; i <= ToDate; i = step(i))
        //            yield return new DateRange(i, 1);
        //    }
        //}

        public IEnumerable<DateTime> Dates
        {
            get
            {
                for (var i = FromDate; i <= ToDate; i = step(i))
                    yield return i;
            }
        }
    }
}
