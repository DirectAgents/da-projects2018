using System;
using System.Collections.Generic;
using System.Globalization;

namespace ClientPortal.Data.DTOs
{
    public class CalenderWeek
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int WeekNumber { get; set; }

        public override string ToString()
        {
            return WeekNumber + ": " + StartDate.ToLongDateString() + " - " + EndDate.ToLongDateString();
        }

        public static List<CalenderWeek> Generate(DateTime startDate, DateTime endDate, DayOfWeek firstDayOfWeek)
        {
            var results = new List<CalenderWeek>();
            Calendar calender = DateTimeFormatInfo.CurrentInfo.Calendar;
            CalenderWeek week = null;

            for (DateTime dt = startDate; dt <= endDate; dt = dt.AddDays(1))
            {
                int weekNumber = calender.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);

                if (week == null || weekNumber != week.WeekNumber)
                {
                    week = new CalenderWeek { StartDate = dt, EndDate = dt, WeekNumber = weekNumber };
                    results.Add(week);
                }
                else
                {
                    week.EndDate = dt;
                }
            }

            return results;
        }
    }
}
