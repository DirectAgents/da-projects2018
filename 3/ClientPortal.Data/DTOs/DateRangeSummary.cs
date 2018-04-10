using System;
using System.Globalization;

namespace ClientPortal.Data.DTOs
{
    public class DateRangeSummary
    {
        public string Name { get; set; }

        public int? Clicks { get; set; }
        public int? Conversions { get; set; }

        public double? ConversionRate
        {
            get
            {
                if (Clicks == null || Conversions == null)
                    return null;
                return (Clicks == 0) ? 0 : Math.Round(((double)(100 * Conversions) / (double)Clicks), 1);
            }
        }

        public decimal? Revenue { get; set; }
        public decimal? ConVal { get; set; }

        public string RevenueFormatted
        {
            //get { return String.Format(new CultureInfo(Culture), "{0:c}", Revenue); }
            get { return String.Format(new CultureInfo("en-US"), "{0:c}", Revenue); } // show all in USD for now
        }
        public string ConValFormatted(string format = "c")
        {
            //return String.Format(new CultureInfo(Culture), "{0:" + format + "}", ConVal);
            return String.Format(new CultureInfo("en-US"), "{0:" + format + "}", ConVal); // show all in USD for now
        }

        public string Currency
        {
            set { Culture = OfferInfo.CurrencyToCulture(value); }
        }
        public string Culture { get; set; }

        public string Link { get; set; }

        public double? PctChg_Clicks { get; set; }
        public double? PctChg_Conversions { get; set; }
        public double? Chg_ConversionRate { get; set; }
        public double? PctChg_Revenue { get; set; }
        public double? PctChg_ConVal { get; set; }

        public static double? ComputePercentChange(int? val1, int? val2)
        {
            if (val1 == null || val2 == null || val1.Value == 0)
                return null;
            var pctChg = (double)(100 * (val2 - val1)) / (double)val1;
            return Math.Round(pctChg, 1);
        }
        public static double? ComputePercentChange(decimal? val1, decimal? val2)
        {
            if (val1 == null || val2 == null || val1.Value == 0)
                return null;
            var pctChg = (double)(100 * (val2 - val1)) / (double)val1;
            return Math.Round(pctChg, 1);
        }
        public static double? ComputeChange(double? val1, double? val2)
        {
            if (val1 == null || val2 == null)
                return null;
            return Math.Round((val2.Value - val1.Value), 1);
        }
    }
}
