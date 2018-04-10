using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Services.Protocols;
using CakeExtracter.Common;
using ClientPortal.Web.Models.Cake;

namespace CakeExtracter
{
    public class CakeMarketing : ICakeMarketing
    {
        private readonly string ApiKey = ConfigurationManager.AppSettings["CakeApiKey"];

        public CakeMarketing(ICakeMarketingCache cakeMarketingCache)
        {
            BatchSize = 25000;
            Cache = cakeMarketingCache;
        }

        public int BatchSize { get; set; }

        public ICakeMarketingCache Cache { get; set; }

        public List<click> Clicks(int advertiserId, DateTime date)
        {
            Console.WriteLine("Extracting clicks for advertiser {0} on {1}..", advertiserId, date);

            var cachedClicks = Cache.Clicks(advertiserId, date);
            if (cachedClicks != null)
            {
                Console.WriteLine("returning {0} cached clicks..", cachedClicks.Count);
                return cachedClicks;
            }

            var reports = new reports();
            const int affiliateId = 0;
            const int offerId = 0;
            const int campaignId = 0;
            const int creativeId = 0;
            const bool includeTests = false;

            Console.WriteLine("Extracting clicks: affiliate_id={0},offer_id={1},campaign_id={2},creative_id={3},click_date={4},advertiser_id={5}", affiliateId, offerId, campaignId, creativeId, date, advertiserId);

            click_report_response result = null;
            int rowCount = 0;
            while (true)
            {
                int count = rowCount;

                Func<click_report_response> getClicks = () =>
                    reports.Clicks(ApiKey, date, date.AddDays(1), affiliateId, advertiserId, offerId, campaignId,
                                   creativeId, includeTests, count + 1, BatchSize);

                var getClicksResult = RetryUtility.Retry(3, 10000, new[] { typeof(SoapException) }, getClicks);

                if (result == null)
                {
                    result = getClicksResult;
                }
                else
                {
                    result.row_count += getClicksResult.row_count;
                    result.clicks = result.clicks.Concat(getClicksResult.clicks).ToArray();
                }

                rowCount += getClicksResult.row_count;

                Console.WriteLine("row count is {0}", rowCount);

                if (getClicksResult.row_count < BatchSize)
                    break;
            }

            Console.WriteLine("total row count is {0}", result.row_count);

            var clicks = result.clicks.ToList();

            Cache.PutClicks(clicks);

            return clicks;
        }

        public IEnumerable<click_report_response> ClicksByConversion(IEnumerable<conversion> conversions)
        {
            var results = new List<click_report_response>();

            results.AddRange(
                conversions
                    .GroupBy(c => new
                    {
                        c.affiliate.affiliate_id,
                        c.offer.offer_id,
                        c.campaign_id,
                        c.creative.creative_id,
                        c.click_date.Value.Date,
                        c.advertiser.advertiser_id
                    })
                    .Select(g =>
                    {
                        var reports = new reports();
                        int affiliateId = g.Key.affiliate_id;
                        int offerId = g.Key.offer_id;
                        int campaignId = g.Key.campaign_id;
                        int advertiserId = g.Key.advertiser_id;
                        int creativeId = g.Key.creative_id;
                        const bool includeTests = false;
                        //const int startAtRow = 0;
                        //const int rowLimit = 0;
                        var clickDate = g.Key.Date;
                        var endDate = clickDate.AddDays(1);

                        Console.WriteLine(
                            "Extracting clicks: affiliate_id={0},offer_id={1},campaign_id={2},creative_id={3},click_date={4},advertiser_id={5}",
                            affiliateId, offerId, campaignId, creativeId, clickDate, advertiserId);

                        click_report_response result = null;
                        int rowCount = 0;
                        while (true)
                        {
                            var r = reports.Clicks(
                                ApiKey, clickDate, endDate, affiliateId, advertiserId,
                                offerId, campaignId, creativeId, includeTests, rowCount + 1, BatchSize);
                            if (result == null)
                            {
                                result = r;
                            }
                            else
                            {
                                result.row_count += r.row_count;
                                result.clicks = result.clicks.Concat(r.clicks).ToArray();
                            }
                            rowCount += r.row_count;
                            Console.WriteLine("row count is {0}", rowCount);
                            if (r.row_count < BatchSize)
                                break;
                        }

                        Console.WriteLine("total row count is {0}", result.row_count);

                        var clickIdsNeeded = g.Select(c => c.click_id);
                        result.clicks = result.clicks.Where(c => clickIdsNeeded.Contains(c.click_id)).ToArray();

                        return result;
                    }));

            return results;
        }

        public List<conversion> Conversions(int advertiserId, DateTime startDate)
        {

            Console.WriteLine("Extracting conversions for advertiser {0} on {1}..", advertiserId, startDate);

            var endDate = startDate.AddDays(1);
            const int affiliateId = 0;
            const int offerId = 0;
            const int campaignId = 0;
            const int creativeId = 0;
            const bool includeTests = false;
            const int startAtRow = 0;
            const int rowLimit = 0;
            const ConversionsSortFields sortFields = ConversionsSortFields.conversion_date;
            const bool isDescending = false;

            var reports = new reports();

            var result = reports.Conversions(
                ApiKey, startDate, endDate, affiliateId, advertiserId, offerId,
                campaignId, creativeId, includeTests, startAtRow, rowLimit, sortFields, isDescending);

            return result.conversions.ToList();
        }
    }
}