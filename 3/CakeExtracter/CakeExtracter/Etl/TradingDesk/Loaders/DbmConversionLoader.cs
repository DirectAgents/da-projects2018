using System;
using System.Collections.Generic;
using System.Data.Entity;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using ClientPortal.Data.Entities.TD;
using ClientPortal.Data.Entities.TD.DBM;

namespace CakeExtracter.Etl.TradingDesk.Loaders
{
    public class DbmConversionLoader : Loader<DataTransferRow>
    {
        private DbmConvConverter convConverter;

        public DbmConversionLoader(int convertToTimezoneOffset)
        {
            this.convConverter = new DbmConvConverter(convertToTimezoneOffset);
        }
        public DbmConversionLoader(DbmConvConverter convConverter)
        {
            this.convConverter = convConverter;
        }

        protected override int Load(List<DataTransferRow> items)
        {
            Logger.Info("Loading {0} DBMConversions..", items.Count);
            //AddUpdate...(items);
            var count = UpsertConversions(items);
            return count;
        }

        private int UpsertConversions(List<DataTransferRow> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var itemCount = 0;
            using (var db = new TDContext())
            {
                foreach (var item in items)
                {
                    var source = new DBMConversion
                    {
                        AuctionID = item.auction_id,
                        EventTime = convConverter.EventTime(item),
                        ViewTime = convConverter.ViewTime(item),
                        RequestTime = convConverter.RequestTime(item),
                        InsertionOrderID = item.insertion_order_id,
                        LineItemID = item.line_item_id,
                        CreativeID = item.creative_id,
                        EventSubType = item.event_sub_type,
                        UserID = item.user_id,
                        AdPosition = item.ad_position,
                        Country = item.country,
                        DMACode = item.dma_code,
                        PostalCode = item.postal_code,
                        GeoRegionID = item.geo_region_id,
                        CityID = item.city_id,
                        OSID = item.os_id,
                        BrowserID = item.browser_id,
                        BrowserTimezoneOffsetMinutes = item.browser_timezone_offset_minutes,
                        NetSpeed = item.net_speed,
                        IP = item.ip
                    };

                    var target = db.Set<DBMConversion>().Find(source.AuctionID, source.EventTime);
                    if (target == null)
                    {
                        db.Conversions.Add(source);
                        addedCount++;
                    }
                    else
                    {
                        var entry = db.Entry(target);
                        entry.State = EntityState.Detached;
                        AutoMapper.Mapper.Map(source, target);
                        entry.State = EntityState.Modified;
                        updatedCount++;
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} DBMConversions ({1} updates, {2} additions)", itemCount, updatedCount, addedCount);
                db.SaveChanges();
            }
            return itemCount;
        }
    }

    public class DbmConvConverter
    {
        private readonly int convertToTimezoneOffset;

        public DbmConvConverter(int convertToTimezoneOffset)
        {
            this.convertToTimezoneOffset = convertToTimezoneOffset;
        }

        public DateTime EventTime(DataTransferRow dtRow)
        {
            var eventTime = Utility.UnixTimeStampToDateTime(dtRow.event_time, this.convertToTimezoneOffset);
            if (TimeZoneInfo.Local.IsDaylightSavingTime(eventTime))
                return eventTime.AddHours(1);
            else
                return eventTime;
        }

        public DateTime? ViewTime(DataTransferRow dtRow)
        {
            if (!dtRow.view_time.HasValue)
                return null;

            var viewTime = Utility.UnixTimeStampToDateTime(dtRow.view_time.Value, convertToTimezoneOffset);
            if (TimeZoneInfo.Local.IsDaylightSavingTime(viewTime))
                return viewTime.AddHours(1);
            else
                return viewTime;
        }

        public DateTime? RequestTime(DataTransferRow dtRow)
        {
            if (!dtRow.request_time.HasValue)
                return null;

            var requestTime = Utility.UnixTimeStampToDateTime(dtRow.request_time.Value, convertToTimezoneOffset);
            if (TimeZoneInfo.Local.IsDaylightSavingTime(requestTime))
                return requestTime.AddHours(1);
            else
                return requestTime;
        }
    }
}
