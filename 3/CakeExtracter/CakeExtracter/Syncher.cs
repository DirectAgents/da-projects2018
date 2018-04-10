using System;
using System.Collections.Generic;
using System.Linq;
using CakeExtracter.Common;
using CakeExtracter.Commands;
using CakeExtracter.Data;
using ClientPortal.Data.Contexts;
using ClientPortal.Web.Models.Cake;
using EntityFramework.Extensions;

namespace CakeExtracter
{
    class Syncher
    {
        private readonly CakeMarketing cake = new CakeMarketing(new CakeMarketingCache { Enabled = true });

        /// <summary>
        /// Ensure the Metrics exists
        /// </summary>
        private static void InitializeMetrics()
        {
            using (var db = new ClientPortalContext())
            {
                if (!db.Metrics.Any(m => m.name == MetricNames.Device))
                {
                    var metric = new Metric { name = MetricNames.Device };
                    db.Metrics.Add(metric);
                }
                if (!db.Metrics.Any(m => m.name == MetricNames.Region))
                {
                    var metric = new Metric { name = MetricNames.Region };
                    db.Metrics.Add(metric);
                }
                db.SaveChanges();
            }
        }

        // TEMP: this method is called by DataWarehouse...

        private void DoSynch(int advertiserId, DateTime fromDate, int addDays, string mode)
        {
            //bool doClicks = (mode == "clicks" || mode == "both");
            //bool doConversions = (mode == "conversions" || mode == "both");
            //bool? conversionsOnly = (mode == "conversions") ? true : (bool?)null;
            //// TODO: integrate to command line flags
            //bool doMainEtl = true;
            //bool doDataWarehouse = false;
            //bool doMetrics = false;

            //List<click> clicks = null;
            //List<conversion> conversions = null;
            //var date = fromDate.AddDays(addDays);

            //if (doMainEtl)
            //{
            //    if (doClicks)
            //    {
            //        clicks = cake.Clicks(advertiserId, date);
            //    }

            //    if (doConversions)
            //    {
            //        conversions = cake.Conversions(advertiserId, date);
            //        DeleteConversions(advertiserId, date);
            //        LoadConversions(conversions);
            //    } 
            //}

            //if (doDataWarehouse)
            //{
            //    var dw = new DataWarehouse();
            //    if (doClicks)
            //    {
            //        dw.UpdateClicks(clicks);
            //    }
            //    if (doConversions)
            //    {
            //        dw.UpdateConversions(conversions);
            //    } 
            //}

            //if (doMetrics)
            //{
            //    DeleteMetricCounts(advertiserId, date, conversionsOnly);
            //    if (doClicks)
            //    {
            //        LoadDeviceCounts(clicks);
            //    }
            //    if (doConversions)
            //    {

            //    }
            //    LoadRegionCounts(advertiserId); 
            //}
        }

        //
        // Metrics
        //

        private static void DeleteMetricCounts(int advertiserId, DateTime date, bool? conversionsOnly)
        {
            var datePlusOne = date.AddDays(1);

            using (var db = new ClientPortalContext())
            {
                var offerIds = db.Offers.Where(o => o.AdvertiserId == advertiserId).Select(o => o.OfferId).ToList();

                var metricCounts = db.MetricCounts.Where(mc => offerIds.Contains(mc.offer_id) &&
                                                               mc.date >= date &&
                                                               mc.date < datePlusOne);
                if (conversionsOnly.HasValue)
                    metricCounts = metricCounts.Where(mc => mc.conversions_only == conversionsOnly.Value);

                int numDeleted = metricCounts.Delete();

                db.SaveChanges();

                Console.WriteLine("deleted {0} MetricCounts", numDeleted);
            }
        }

        private static void LoadDeviceCounts(List<click> clickReport)
        {
            if (clickReport.Count > 0)
            {
                using (var db = new ClientPortalContext())
                {
                    var deviceMetric = db.Metrics.Where(m => m.name == MetricNames.Device).First();

                    var click0 = clickReport[0];
                    var date = new DateTime(click0.click_date.Year, click0.click_date.Month, click0.click_date.Day);

                    var deviceGroups = clickReport.GroupBy(c => c.device.device_name);

                    // Ensure that all MetricValues exists
                    foreach (var dg in deviceGroups)
                    {
                        if (!deviceMetric.MetricValues.Any(mv => mv.name == dg.Key))
                        {
                            Console.WriteLine("adding new MetricValue: {0}", dg.Key);

                            var metricValue = new MetricValue()
                                {
                                    name = dg.Key,
                                    code = dg.First().device.device_id.ToString()
                                };
                            deviceMetric.MetricValues.Add(metricValue);
                        }
                    }

                    db.SaveChanges();

                    Console.WriteLine("adding MetricCounts..");

                    // Add MetricCounts
                    foreach (var dg in deviceGroups)
                    {
                        var metricValue = db.MetricValues.First(mv => mv.name == dg.Key);
                        var offerGroups = dg.GroupBy(d => d.offer.offer_id);

                        foreach (var og in offerGroups)
                        {
                            var metricCount = new MetricCount()
                                {
                                    offer_id = og.Key,
                                    date = date,
                                    conversions_only = false,
                                    count = og.Count()
                                };
                            metricValue.MetricCounts.Add(metricCount);
                        }

                        db.SaveChanges();
                    }
                }
            }
        }

        private void LoadRegionCounts(int advertiserId)
        {

        }

        private void LoadRegionCounts(click_report_response clickReport, conversion_report_response conversionReport)
        {
            if (conversionReport.conversions.Length > 0)
            {
                using (var db = new ClientPortalContext())
                {
                    var conversion0 = conversionReport.conversions[0];
                    var offerId = conversion0.offer.offer_id;
                    var date = new DateTime(conversion0.conversion_date.Year, conversion0.conversion_date.Month,
                                            conversion0.conversion_date.Day);

                    // Match converions to clicks
                    var conversionsAndClicksQuery = from cv in conversionReport.conversions
                                                    from ck in clickReport.clicks
                                                                          .Where(c => c.click_id == cv.click_id)
                                                                          .DefaultIfEmpty()
                                                    select new { Conversion = cv, Click = ck };

                    var conversionsAndClicks = conversionsAndClicksQuery
                        .ToArray();

                    // Find unmatched
                    var unmatched = conversionsAndClicks
                        .Where(c => c.Click == null)
                        .ToArray();

                    var conversionsNeedingClick = unmatched.Select(c => c.Conversion);

                    // Extract clicks to fix unmatched
                    var extractedClicks = cake.ClicksByConversion(conversionsNeedingClick)
                        .SelectMany(c => c.clicks)
                        .ToArray();

                    // Fix unmatched
                    var fixedUp = unmatched.Select(c => new
                        {
                            c.Conversion,
                            Click = extractedClicks.FirstOrDefault(extracted => extracted.click_id == c.Conversion.click_id)
                        })
                        .ToArray();

                    // Find matched
                    var matched = conversionsAndClicks.Where(c => c.Click != null);

                    // Combine matched and fixed
                    var conversions = new[] { matched, fixedUp }.SelectMany(c => c).ToArray();

                    // Group by country code and region code
                    var groups = conversions.Where(c => c.Click != null)
                        .GroupBy(c => new { Country = c.Click.country.country_code, Region = c.Click.region.region_code })
                        .ToArray();

                    // Check the count
                    int totalConversions = conversionReport.conversions.Count();
                    int countedMetrics = groups.Sum(c => c.Count());
                    int uncountedMetrics = totalConversions - countedMetrics;
                    if (uncountedMetrics == 0)
                        Console.WriteLine("Counts Match!");
                    else
                        Console.WriteLine("Counts are off: {0} - {1} = {2}", totalConversions, countedMetrics, uncountedMetrics);

                    // Get the metric entity for Region
                    var metric = db.Metrics.Single(m => m.name == MetricNames.Region);

                    // Ensure that a MetricValue for each group exists
                    foreach (var regionGroup in groups)
                    {
                        var existingMetric = metric.MetricValues.FirstOrDefault(c => c.name == regionGroup.Key.Region && c.code == regionGroup.Key.Country);

                        if (existingMetric == null)
                        {
                            Console.WriteLine("adding new MetricValue: {0}", regionGroup.Key.Region + "," + regionGroup.Key.Country);

                            var metricValue = new MetricValue()
                            {
                                name = regionGroup.Key.Region,
                                code = regionGroup.Key.Country
                            };

                            metric.MetricValues.Add(metricValue);
                        }

                        db.SaveChanges();
                    }

                    Console.WriteLine("adding MetricCounts..");

                    // Add a MetricCount for each region
                    foreach (var regionGroup in groups)
                    {
                        var metricValue = metric.MetricValues.First(c => c.name == regionGroup.Key.Region && c.code == regionGroup.Key.Country);

                        var offerGroups = regionGroup.GroupBy(d => d.Conversion.offer.offer_id);

                        foreach (var og in offerGroups)
                        {
                            var metricCount = new MetricCount()
                            {
                                offer_id = og.Key,
                                date = date,
                                conversions_only = false,
                                count = og.Count()
                            };

                            metricValue.MetricCounts.Add(metricCount);
                        }

                        db.SaveChanges();
                    }
                }
            }
        }

        //
        // Conversions
        //

        private void DeleteConversions(int advertiserId, DateTime date)
        {
            using (var db = new UsersContext())
            {
                const string deleteSql = "delete from Conversion where advertiser_advertiser_id = {0} and conversion_date between {1} and {2}";
                int rowCount = db.Database.ExecuteSqlCommand(deleteSql, advertiserId, date, date.AddDays(1));
                Console.WriteLine("deleted {0} conversions", rowCount);
            }
        }

        private static void LoadConversions(List<conversion> conversions)
        {
            Console.WriteLine("loading {0} conversions..", conversions.Count);

            int total = conversions.Count;
            int count = 0;
            foreach (var set in conversions.InBatches(2000))
            {
                using (var db = new UsersContext())
                {
                    count += set.Count;

                    Console.WriteLine("saving {0}/{1} conversions..", count, total);

                    set.ForEach(c => db.Conversions.Add(c));
                    db.SaveChanges();
                }
            }
        }

        //
        // Clicks
        //

        //private void LoadClicks(click_report_response result)
        //{
        //    Console.WriteLine("loading {0} clicks..", result.row_count);
        //    int total = result.row_count;
        //    int count = 0;
        //    foreach (var set in result.clicks.InSetsOf(2000))
        //    {
        //        count += set.Count;
        //        Console.WriteLine("saving {0}/{1} clicks..", count, total);
        //        using (var db = new UsersContext())
        //        {
        //            set.ForEach(c => db.Clicks.Add(c));
        //            db.SaveChanges();
        //        }
        //    }
        //}
    }
}

//private static click_report_response ExtractConversionsFromFile(int advertiserId, DateTime startDate)
//{
//    string fileName = string.Format(baseDir + "conversions_{0}_{1}.txt", advertiserId, startDate.ToString("MM_dd_yyyy"));
//    if (!File.Exists(fileName))
//        return null;
//    Console.WriteLine("Extracting conversions from file: {0}..", fileName);
//    var serializer = new XmlSerializer(typeof(click_report_response));
//    var reader = new StreamReader(fileName);
//    var result = (click_report_response)serializer.Deserialize(reader);
//    return result;
//}
//public click_report_response ExtractCakeClicks(
//    string api_key,
//    System.DateTime the_date,
//    int affiliate_id,
//    int advertiser_id,
//    int offer_id,
//    int campaign_id,
//    int creative_id,
//    bool include_tests,
//    int start_at_row,
//    int row_limit)
//{
//    string cachePath = "c:\\CakeCache\\advertiser_offer\\"
//                       + string.Format("{0}_{1}", advertiser_id, offer_id)
//                       + "\\affiliate_campaign\\"
//                       + string.Format("{0}_{1}", affiliate_id, campaign_id)
//                       + "\\"
//                       + string.Format("clicks_{0}_{1}_{3}.xml");
//    using (var fileStrean = File.Create(cachePath))
//    {      
//    }
//}