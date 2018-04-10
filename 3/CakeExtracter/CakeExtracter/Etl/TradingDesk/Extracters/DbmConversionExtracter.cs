using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CakeExtracter.Common;
using CsvHelper;
using Google;
using System.Configuration;
using Newtonsoft.Json;
using DBM.Entities;
using DirectAgents.Domain.Contexts;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class DbmConversionExtracter : Extracter<DataTransferRow>
    {
        private readonly DateRange dateRange;
        private readonly int? insertionOrderId; // null for all
        private readonly bool includeViewThrus; // (false = only include click-thrus)
        private readonly IEnumerable<string> advertiserIds;

        public DbmConversionExtracter(DateRange dateRange, IEnumerable<string> advertiserIds, int? insertionOrderId, bool includeViewThrus)
        {
            this.dateRange = dateRange;
            this.advertiserIds = advertiserIds;
            this.insertionOrderId = insertionOrderId;
            this.includeViewThrus = includeViewThrus;
        }

        protected override void Extract()
        {
            string includeWhat = includeViewThrus ? "all" : "click-thru";
            string bucket = (advertiserIds.Count() == 1) ? advertiserIds.First() : "all buckets";
            Logger.Info("Extracting {0} Conversions from Data Transfer Files for {1} from {2} to {3}", includeWhat, bucket, dateRange.FromDate, dateRange.ToDate);
            if (insertionOrderId.HasValue)
                Logger.Info("InsertionOrder {0}", insertionOrderId);
            var items = EnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<DataTransferRow> EnumerateRows()
        {
            if (advertiserIds == null || !advertiserIds.Any())
                yield break; // skip city lookup, etc

            var credential = DbmCloudStorageExtracter.CreateCredential();
            var service = DbmCloudStorageExtracter.CreateStorageService(credential);

            //var listRequest = service.Objects.List(bucketName);
            //var bucketObjects = listRequest.Execute();

            foreach (var date in dateRange.Dates)
            {
                string filename = string.Format("log/{0}.0.conversion.0.csv", date.ToString("yyyyMMdd"));
                string geoFilename = String.Format("entity/{0}.0.GeoLocation.json", date.ToString("yyyyMMdd"));
                Logger.Info("Date: {0}", date);

                var cityRequest = service.Objects.Get("gdbm-public", geoFilename);
                Google.Apis.Storage.v1.Data.Object cityObj = cityRequest.Execute();
                var cityStream = DbmCloudStorageExtracter.GetStreamForCloudStorageObject(cityObj, credential);
                var lookupTable = CreateCityLookup(cityStream);

                foreach (var advertiserId in advertiserIds)
                {

                    Logger.Info("Advertiser Id {0}", advertiserId);
                    string thisBucket = String.Format("gdbm-479-{0}", advertiserId);
                    var request = service.Objects.Get(thisBucket,filename);
                    Google.Apis.Storage.v1.Data.Object obj;

                    try
                    {
                        obj = request.Execute();
                    }
                    catch (GoogleApiException)
                    {
                        continue;
                    }

                    if (obj != null)
                    {
                        var stream = DbmCloudStorageExtracter.GetStreamForCloudStorageObject(obj, credential);
                        using (var reader = new StreamReader(stream))
                        {
                            foreach (var row in EnumerateRowsStatic(reader))
                            {
                                var res = lookupTable.Where(c => c.id == row.city_id).FirstOrDefault();

                                if (res != null)
                                {
                                    var duplicateCount = lookupTable.Where(c => c.short_name == res.short_name && c.country_name == res.country_name).Count();
                                    //Logger.Info("City {0} in Country {1} with ID {2}", res.city_name, res.country_name, row.city_id);
                                    row.setAttributes(res.city_name, res.country_name, res.short_name, duplicateCount);
                                }
                                yield return row;
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<DataTransferRow> EnumerateRowsStatic(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                csv.Configuration.WillThrowOnMissingField = false;
                var csvRows = csv.GetRecords<DataTransferRow>().ToList();
                for (int i = 0; i < csvRows.Count; i++)
                {
                    if (insertionOrderId.HasValue && insertionOrderId.Value != csvRows[i].insertion_order_id)
                        continue;
                    if (!includeViewThrus)
                    {
                        if (csvRows[i].event_sub_type == "postview")
                            continue;
                        Logger.Info("Extracting a conversion"); //only log this if we're just extracting clickthrus
                    }
                    yield return csvRows[i];
                }
            }
        }

        public List<GeoLocation> CreateCityLookup(Stream stream)
        {

            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(sr))
            {
                var jsonObjs = serializer.Deserialize<List<GeoLocation>>(jsonReader);
                return jsonObjs;
            }
        }
    }

    // TODO: handle blank event_time / auction_id... skip that row?

    public class DataTransferRow
    {
        public string auction_id { get; set; }
        public long event_time { get; set; }
        public long? view_time { get; set; }
        public long? request_time { get; set; }
        public int? insertion_order_id { get; set; }
        public int? line_item_id { get; set; }
        public int? creative_id { get; set; }

        public string event_type { get; set; }
        public string event_sub_type { get; set; }
        public string user_id { get; set; }
        public int? ad_position { get; set; }
        public string ip { get; set; }
        public string country { get; set; }
        public int? dma_code { get; set; }
        public string postal_code { get; set; }
        public int? geo_region_id { get; set; }
        public int? city_id { get; set; }
        public int? os_id { get; set; }
        public int? browser_id { get; set; }
        public int? browser_timezone_offset_minutes { get; set; }
        public int? net_speed { get; set; }

        public string city_name { get; set; }
        public string country_name { get; set; }
        public string advertiser_id { get; set; }
        public bool unique_city_name { get; set; }
        public string short_name { get; set; }

        public void setAttributes(string city, string country, string shortName, int uniqueCount)
        {
            city_name = city;
            country_name = country;
            short_name = shortName;
            unique_city_name = (uniqueCount <= 1);
            //advertiser_id = insertionOrderLookup[insertion_order_id];
        }
    }

    public class GeoLocation
    {
        public int id { get; set; }
        public string country_code { get; set; }
        public string region_code { get; set; }
        public string canonical_name { get; set; }
        public string postal_code { get; set; }
        public string geo_code { get; set; }
        public string country_name
        {
            get
            {
                return canonical_name.Split(',').Last();
            }
        }
        public string city_name
        {
            get
            {
                return canonical_name.Replace(country_name, "").TrimEnd(',');
            }
        }
        public string short_name
        {
            get
            {
                return city_name.Split(',').First();
            }
        }
    }

}
