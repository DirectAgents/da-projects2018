using System;

namespace ClientPortal.Data.DTOs
{
    public class SimpleSearchStat
    {
        public string Title { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public int ROAS { get; set; }
        public decimal Margin { get; set; }
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int Orders { get; set; }

        public int ViewThrus { get; set; }
        public decimal ViewThruRev { get; set; }

        public int CassConvs { get; set; }
        public double CassConVal { get; set; }

        public decimal CPO { get; set; }
        public decimal CTR { get; set; }
        public decimal OrderRate { get; set; }
        public decimal CPC { get; set; }

        public int Calls { get; set; }
        public int TotalLeads { get; set; }
        public decimal CPL { get; set; }
    }

    public class SearchStatVals
    {
        public SearchStatVals() { }
        public SearchStatVals(SearchStatVals vals)
        {
            Impressions = vals.Impressions;
            Clicks = vals.Clicks;
            Orders = vals.Orders;
            ViewThrus = vals.ViewThrus;
            RevPerViewThru = vals.RevPerViewThru;
            CassConvs = vals.CassConvs;
            CassConVal = vals.CassConVal;
            Revenue = vals.Revenue;
            Cost = vals.Cost;
            Calls = vals.Calls;
        }

        public bool AllZeros(bool checkCassConvs)
        {
            return (Impressions == 0 && Clicks == 0 && Orders == 0 && ViewThrus == 0 && Revenue == 0 && Cost == 0 && Calls == 0
                    && (!checkCassConvs || (CassConvs == 0 && CassConVal == 0)));
            // if we end up showing CassConvs in the portal, set checkCassConvs==true
            // ... in device breakdown, will produce a row showing Google CassConvs, with device=="All"
        }

        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int Orders { get; set; } // also used for Leads
        public int ViewThrus { get; set; }

        public decimal RevPerViewThru { set; private get; }
        public decimal ViewThruRev
        {
            get { return ViewThrus * RevPerViewThru; }
        }

        public int CassConvs { get; set; }
        public double CassConVal { get; set; }

        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }

        //private readonly int _calls;
        //public int Calls { get { return _calls; } }
        public int Calls { get; set; }
        public int TotalLeads
        {
            get { return Orders + Calls; }
        }

        public decimal OrderRate // TODO: replace this everywhere with ConvRate, then remove this?
        {
            get { return Clicks == 0 ? 0 : Math.Round((decimal)100 * Orders / Clicks, 2); }
        }
        public decimal ConvRate
        {
            get { return Clicks == 0 ? 0 : Math.Round((decimal)100 * TotalLeads / Clicks, 2); }
        }

        public decimal CPO
        {
            get { return Orders == 0 ? 0 : Math.Round(Cost / Orders, 2); }
        }
        public decimal CPL
        {
            get { return TotalLeads == 0 ? 0 : Math.Round(Cost / TotalLeads, 2); }
        }

        //public int ROI
        //{
        //    get { return Cost == 0 ? 0 : (int)Math.Round(100 * Revenue / Cost) - 100; }
        //}
        public int ROAS
        {
            get { return Cost == 0 ? 0 : (int)Math.Round(100 * Revenue / Cost); }
        }
        public decimal Margin
        {
            get { return Revenue - Cost; }
        }
        public decimal RevenuePerOrder
        {
            get { return Orders == 0 ? 0 : Math.Round(Revenue / Orders, 2); }
        }
        public decimal CPC
        {
            get { return Clicks == 0 ? 0 : Math.Round(Cost / Clicks, 2); }
        }
        public decimal CTR
        {
            get { return Impressions == 0 ? 0 : Math.Round((decimal)100 * Clicks / Impressions, 2); }
        }
        //public decimal OrdersPerDay
        //{
        //    get { return Days == 0 ? 0 : Math.Round((decimal)Orders / Days, 2); }
        //}
    }

    public class SearchStat : SearchStatVals
    {
        // --- TEMP ---
        public int Impressions_() { return Impressions; }
        public int Clicks_() { return Clicks; }
        public int Orders_() { return Orders; }
        public int ViewThrus_() { return ViewThrus; }
        public decimal ViewThruRev_() { return ViewThruRev; }
        public int CassConvs_() { return CassConvs; }
        public double CassConVal_() { return CassConVal; }
        public decimal Revenue_() { return Revenue; }
        public decimal Cost_() { return Cost; }
        public int Calls_() { return Calls; }
        public int TotalLeads_() { return TotalLeads; }
        //public decimal OrderRate_() { return OrderRate; }
        //public decimal ConvRate_() { return ConvRate; }
        //public decimal CPO_() { return CPO; }
        //public decimal CPL_() { return CPL; }
        //public int ROAS_() { return ROAS; }
        //public decimal Margin_() { return Margin; }
        //public decimal RevenuePerOrder_() { return RevenuePerOrder; }
        //public decimal CPC_() { return CPC; }
        //public decimal CTR_() { return CTR; }

        public int PrevImpressions() { return (Prev == null) ? 0 : Prev.Impressions; }
        public int PrevClicks() { return (Prev == null) ? 0 : Prev.Clicks; }
        public int PrevOrders() { return (Prev == null) ? 0 : Prev.Orders; }
        public decimal PrevCost() { return (Prev == null) ? 0 : Prev.Cost; }
        public decimal PrevRevenue() { return (Prev == null) ? 0 : Prev.Revenue; }
        public decimal PrevTotalLeads() { return (Prev == null) ? 0 : Prev.TotalLeads; }
        // --- end TEMP ---

        public SearchStatVals Prev { get; set; } // for the prev year's stats

        public int? CampaignId { get; set; }

        public string Campaign { get; set; }
        public string Channel { get; set; }
        public string Title { get; set; }
        public string Range { get; set; }

        private string _network;
        public string Network
        {
            set { _network = value; }
            get
            {
                switch (_network)
                {
                    case "S":
                        return "Search";
                    case "D":
                        return "Display";
                    case "V":
                        return "YT Videos";
                    case "Y":
                        return "YT Search";
                    default:
                        return _network;
                }
            }
        }
        private string _device;
        public string Device
        {
            set { _device = value; }
            get
            {
                switch (_device)
                {
                    case "C":
                        return "Computer";
                    case "M":
                        return "Mobile";
                    case "T":
                        return "Tablet";
                    case "O":
                        return "Other";
                    case "A":
                        return "All";
                    default:
                        return _device;
                }
            }
        }
        public string DeviceAbbrev
        {
            get { return _device; }
        }
        public string DeviceAndTitle
        {
            set
            {
                Device = value;
                Title = Device; // the full name
            }
        }

        private string _clickType;
        public string ClickType
        {
            set { _clickType = value; }
            get
            {
                switch (_clickType)
                {
                    case "H":
                        return "Headline";
                    case "S":
                        return "Sitelink";
                    case "O":
                        return "Offer";
                    case "C":
                        return "Call";
                    case "P":
                        return "PLA";
                    case "Q":
                        return "PLA Coupon";
                    //case "G":
                    //case "M":
                    //case "I":
                    default:
                        return _clickType;
                }
            }
        }

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
        internal DayOfWeek WeekStartDay {
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

        private bool _yoy;
        internal bool YoY
        {
            set { _yoy = value; }
        }

        internal DateTime MonthByMaxDate
        {
            set
            {
                this.EndDate = value;
                this.Days = this.EndDate.Day;

                this.Range = ToRangeName(this.EndDate, false, true, !_yoy); // not sure if need to includeYear here
                this.Title = ToRangeName(this.EndDate, false, false, !_yoy);
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

        internal DateTime SingleDate // (note: setting (end) Date not required)
        {
            set
            {
                this.EndDate = value;
                this.Days = 1;
                // this.Range = ?
                // this.Title = ?
            }
        }

        // will set Title only if value is not null
        internal string TitleIfNotNull
        {
            set { if (value != null) this.Title = value; }
        }

        // --- Constructors ---

        // call this when using one of the above initializers
        public SearchStat() { }

        // use for one day's stats, or for a week- ending on the specified date
        //public SearchStat(bool isWeekly, int year, int month, int day, int impressions, int clicks, int orders, decimal revenue, decimal cost, string title = null)
        //{
        //    this.EndDate = new DateTime(year, month, day);
        //    this.Range = ToRangeName(this.EndDate, isWeekly, true);
        //    this.Title = title ?? ToRangeName(this.EndDate, isWeekly);

        //    if (isWeekly)
        //        Days = 7;
        //    else
        //        Days = day;

        //    this.Val.Impressions = impressions;
        //    this.Val.Clicks = clicks;
        //    this.Val.Orders = orders;
        //    this.Val.Revenue = revenue;
        //    this.Val.Cost = cost;
        //}

        // --- Private methods ---

        private string ToRangeName(DateTime date, bool isWeekly, bool prependYMD, bool includeYear)
        {
            if (isWeekly)
            {
                DateTime weekStart = date.AddDays(-6);
                return ToRangeName(weekStart, date, prependYMD, includeYear);
            }
            else
            {
                string dateFormat = includeYear ? "MMM-yy" : "MMM";
                return (prependYMD ? date.ToString("yyyyMMdd") + " " : "") + date.ToString(dateFormat);
            }
        }

        private string ToRangeName(DateTime start, DateTime end, bool prependYMD = false, bool includeYear = false)
        {
            string dateFormat = includeYear ? "MM/dd/yy" : "MM/dd";
            return (prependYMD ? start.ToString("yyyyMMdd") + " " : "") + start.ToString(dateFormat) + " - " + end.ToString(dateFormat);
        }
    }
}
