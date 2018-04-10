using System;
using System.Collections.Generic;
using System.Linq;
using CakeExtracter.CakeMarketingApi.Clients;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;

namespace CakeExtracter.CakeMarketingApi
{
    public static class CakeMarketingUtility
    {
        public static List<Advertiser> Advertisers(int advertiserId = 0)
        {
            var client = new AdvertisersClient();
            var request = new AdvertisersRequest
            {
                advertiser_id = advertiserId
            };
            var response = client.Advertisers(request);
            if (response != null)
                return response.Advertisers;
            else
            {
                Logger.Error(new Exception("Could not retrieve Advertiser(s)."));
                return new List<Advertiser>();
            }
        }

        public static List<Affiliate> Affiliates(int affiliateId = 0)
        {
            var client = new AffiliatesClient();
            var request = new AffiliatesRequest
            {
                affiliate_id = affiliateId
            };
            var response = client.Affiliates(request);
            if (response != null)
                return response.Affiliates;
            else
            {
                Logger.Error(new Exception("Could not retrieve Affiliate(s)."));
                return new List<Affiliate>();
            }
        }

        public static List<int> OfferIds(int advertiserId = 0)
        {
            var client = new OffersClient();
            var request = new OffersRequest
            {
                advertiser_id = advertiserId
            };
            var response = client.Offers(request);
            if (response == null || response.Offers == null)
            {
                Logger.Info("Unable to retrieve offers. Trying again...");
                response = client.Offers(request);
            }
            var offerIds = response.Offers.Select(c => c.OfferId);
            return offerIds.ToList();
        }

        public static List<Offer> Offers(int advertiserId = 0, int offerId = 0)
        {
            var client = new OffersClient();
            var request = new OffersRequest()
            {
                advertiser_id = advertiserId,
                offer_id = offerId
            };
            var response = client.Offers(request);
            return response.Offers;
        }

        public static List<Campaign> Campaigns(int offerId = 0, int campaignId = 0)
        {
            var client = new CampaignsClient();
            var request = new CampaignsRequest
            {
                offer_id = offerId,
                campaign_id = campaignId
            };
            var response = client.Campaigns(request);
            return response.Campaigns;
        }

        public static List<Creative> Creatives(int offerId)
        {
            var client = new CreativesClient();
            var request = new CreativesRequest
            {
                offer_id = offerId
            };
            var response = client.Creatives(request);
            return response.Creatives;
        }

        // Basically, get active(?) offers and their stats for the specified dateRange
        // Remember: for one day, use from=date and end=date+1
        public static List<OfferSummary> OfferSummaries(DateRange dateRange, int advertiserId = 0, int offerId = 0)
        {
            var client = new OfferSummariesClient();
            var request = new OfferSummariesRequest
            {
                start_date = dateRange.FromDate.ToString("MM/dd/yyyy"),
                end_date = dateRange.ToDate.ToString("MM/dd/yyyy"),
                advertiser_id = advertiserId,
                offer_id = offerId
            };
            var response = client.OfferSummaries(request);
            if (response == null || response.Offers == null)
            {
                Logger.Info("Unable to retrieve offers summaries. Trying again...");
                response = client.OfferSummaries(request);
            }
            if (response != null)
                return response.Offers;
            else
                return new List<OfferSummary>();
        }

        public static List<CampaignSummary> CampaignSummaries(DateRange dateRange, int offerId = 0)
        {
            var client = new CampaignSummariesClient();
            var request = new CampaignSummariesRequest
            {
                start_date = dateRange.FromDate.ToString("MM/dd/yyyy"),
                end_date = dateRange.ToDate.ToString("MM/dd/yyyy"),
                site_offer_id = offerId
            };
            var response = client.CampaignSummaries(request);
            if (response == null || response.Campaigns == null)
            {
                Logger.Info("Unable to retrieve campaign summaries. Trying again...");
                response = client.CampaignSummaries(request);
            }
            if (response != null)
                return response.Campaigns;
            else
                return new List<CampaignSummary>();
        }

        public static List<DailySummary> DailySummaries(DateRange dateRange, int advertiserId, int offerId, int creativeId, int affiliateId)
        {
            var client = new DailySummariesClient();
            var request = new DailySummariesRequest
            {
                advertiser_id = advertiserId,
                offer_id = offerId,
                creative_id = creativeId,
                affiliate_id = affiliateId,
                start_date = dateRange.FromDate.ToString("MM/dd/yyyy"),
                end_date = dateRange.ToDate.ToString("MM/dd/yyyy")
            };
            var response = client.DailySummaries(request);
            if (response != null)
                return response.DailySummaries;
            else
                return new List<DailySummary>();
        }

        public static List<Click> Clicks(DateRange dateRange, int advertiserId, int offerId, out int rowCount)
        {
            int startAtRow = 1;
            int rowLimitForOneCall = 5000;
            List<Click> result = new List<Click>();
            while (true)
            {
                int total = 0;
                var request = new ClicksRequest
                {
                    start_date = dateRange.FromDate.ToString("MM/dd/yyyy"),
                    end_date = dateRange.ToDate.ToString("MM/dd/yyyy"),
                    advertiser_id = advertiserId,
                    offer_id = offerId,
                    row_limit = rowLimitForOneCall,
                    start_at_row = startAtRow
                };

                var client = new ClicksClient();
                var response = client.Clicks(request);

                if (!response.Success)
                    throw new Exception("ClicksClient failed");

                total += response.Clicks.Count;
                result.AddRange(response.Clicks);
                if (total >= response.RowCount)
                {
                    Logger.Info("Extracted a total of {0}, returning result..", total);
                    break;
                }
                startAtRow += rowLimitForOneCall;
                Logger.Info("Extracted a total of {0}, checking for more, starting at row {1}..", total, startAtRow);
            }
            rowCount = result.Count;
            return result;
        }

        public static IEnumerable<Click> EnumerateClicks(DateRange dateRange, int advertiserId, int offerId)
        {
            // initialize to start at the first row
            int startAtRow = 1;

            // hard code an upper limit for the max number of rows to be returned in one call
            int rowLimitForOneCall = 5000;

            DateTime? lastDateTime = null;
            bool done = false;
            int total = 0;
            while (!done)
            {
                // prepare the request
                var request = new ClicksRequest
                {
                    start_date = dateRange.FromDate.ToString(),
                    end_date = dateRange.ToDate.ToString(),
                    advertiser_id = advertiserId,
                    offer_id = offerId,
                    row_limit = rowLimitForOneCall,
                    start_at_row = startAtRow
                };

                // create the client, call the service and check the response
                var client = new ClicksClient();
                ClickReportResponse response;
                try
                {
                    response = client.Clicks(request);
                }
                catch (Exception)
                {
                    Logger.Warn("Caught an exception while extracting clicks, bailing out..");
                    yield break;
                }

                if (response == null)
                    throw new Exception("Clicks client returned null response");

                if (!response.Success)
                    throw new Exception("ClicksClient failed");

                // update the running total
                total += response.Clicks.Count;

                // return result
                foreach (var click in response.Clicks)
                {
                    yield return click;
                }

                if (total >= response.RowCount) // all rows have been extracted
                    done = true;
                else if (response.Clicks.Count == 0)
                {
                    Logger.Warn("Clicks client returned 0 clicks. Total extracted: {0} RowCount (expected): {1}", total, response.RowCount);
                    // April 2014: the api doesn't return anything beyond row 100,000 (no matter what start_at_row is) so...
                    if (total >= 100000 && lastDateTime.HasValue)
                    {
                        Logger.Warn("Recursively calling EnumerateClicks, starting at {0}", lastDateTime.Value);
                        // 3/25/15: noticed the api is now returning clicks in reverse chronological order
                        DateRange newDateRange = new DateRange(dateRange.FromDate, lastDateTime.Value, false);
                        var moreClicks = EnumerateClicks(newDateRange, advertiserId, offerId);
                        foreach (var click in moreClicks)
                            yield return click;
                    }
                    else
                    {   // If lastDateTime==null, it must have happened with startAtRow was 1; so we're not able to decrease the dateRange.
                        // If total<100,000, the api isn't returning all the rows it says there are for some other reason.
                        Logger.Warn("Bailing out of EnumerateClicks");
                    }
                    yield break;
                }
                else
                {
                    lastDateTime = response.Clicks[response.Clicks.Count - 1].ClickDate; // remember the last clickdate retrieved
                    startAtRow += rowLimitForOneCall;
                    Logger.Info("Extracted a total of {0} rows, checking for more, starting at row {1}..", total, startAtRow);
                }
            }

            Logger.Info("Extracted a total of {0}, done.", total);
        }

        public static List<Conversion> Conversions(DateRange dateRange, int advertiserId, int offerId)
        {
            var request = new ConversionsRequest
            {
                start_date = dateRange.FromDate.ToString("MM/dd/yyyy"),
                end_date = dateRange.ToDate.ToString("MM/dd/yyyy"),
                advertiser_id = advertiserId,
                offer_id = offerId
            };
            var client = new ConversionsClient();
            var response = client.Conversions(request);
            return response.Conversions.ToList();
        }

        public static List<EventConversion> EventConversions(DateRange dateRange, int advertiserId, int offerId)
        {
            var request = new EventConversionsRequest
            {
                start_date = dateRange.FromDate.ToString("MM/dd/yyyy"),
                end_date = dateRange.ToDate.ToString("MM/dd/yyyy"),
                brand_advertiser_id = advertiserId,
                site_offer_id = offerId
            };
            var client = new EventConversionsClient();
            var response = client.EventConversions(request);
            return response.EventConversions.ToList();
        }

        public static List<Traffic> Traffic(DateRange dateRange)
        {
            var request = new TrafficRequest
            {
                start_date = dateRange.FromDate.ToString("MM/dd/yyyy"),
                end_date = dateRange.ToDate.ToString("MM/dd/yyyy"),
            };
            var client = new TrafficClient();
            var response = client.Traffic(request);
            return response.Traffics.ToList();
        }
    }
}