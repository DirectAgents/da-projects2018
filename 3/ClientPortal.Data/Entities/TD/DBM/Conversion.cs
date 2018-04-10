using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Entities.TD.DBM
{
    public class DBMConversion
    {
        public string AuctionID { get; set; }
        public DateTime EventTime { get; set; }
        public DateTime? ViewTime { get; set; }
        public DateTime? RequestTime { get; set; }
        public int? InsertionOrderID { get; set; }
        public int? LineItemID { get; set; }
        public int? CreativeID { get; set; }

        public string EventSubType { get; set; }
        public string UserID { get; set; }
        public int? AdPosition { get; set; }
        public string Country { get; set; }
        public int? DMACode { get; set; }
        public string PostalCode { get; set; }
        public int? GeoRegionID { get; set; }
        public int? CityID { get; set; }
        public int? OSID { get; set; }
        public int? BrowserID { get; set; }
        public int? BrowserTimezoneOffsetMinutes { get; set; }
        public int? NetSpeed { get; set; }

        public string IP { get; set; }
    }
}
