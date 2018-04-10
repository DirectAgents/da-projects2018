using System;
using System.Collections.Generic;
using System.Linq;
using Amazon;
using Amazon.Entities;
using CakeExtracter.Common;
using DirectAgents.Domain.Entities.CPProg;
using Newtonsoft.Json;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public abstract class AmazonApiExtracter<T> : Extracter<T>
    {
        protected readonly AmazonUtility _amazonUtility;
        protected readonly DateRange dateRange;
        protected readonly string clientId;
        protected readonly string campaignFilter;
        protected readonly string campaignFilterOut;

        protected bool HasCampaignFilter() {
            return !String.IsNullOrEmpty(campaignFilter) || !String.IsNullOrEmpty(campaignFilterOut);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonApiExtracter{T}"/> class.
        /// </summary>
        /// <param name="amazonUtility">The amazon utility.</param>
        /// <param name="date">The date.</param>
        /// <param name="clientId">The client identifier.</param>
        public AmazonApiExtracter(AmazonUtility amazonUtility, DateRange dateRange, string clientId, string campaignFilter = null, string campaignFilterOut = null)
        {
            this._amazonUtility = amazonUtility;
            this.dateRange = dateRange;
            this.clientId = clientId;
            this.campaignFilter = campaignFilter;
            this.campaignFilterOut = campaignFilterOut;
        }

        // apply filters if there are any
        protected List<AmazonCampaign> LoadCampaignsFromAmazonAPI()
        {
            List<AmazonCampaign> campaigns = _amazonUtility.GetCampaigns(clientId);
            if (!String.IsNullOrEmpty(campaignFilter))
                campaigns = campaigns.Where(x => x.name.Contains(campaignFilter)).ToList();
            if (!String.IsNullOrEmpty(campaignFilterOut))
                campaigns = campaigns.Where(x => !x.name.Contains(campaignFilterOut)).ToList();

            return campaigns;
        }

        public static void SetCPProgStats(StatsSummary cpProgStats, Amazon.Entities.StatSummary amazonStats)
        {
            cpProgStats.Cost = amazonStats.cost;
            cpProgStats.Impressions = amazonStats.impressions;
            cpProgStats.Clicks = amazonStats.clicks;
            cpProgStats.PostClickConv = amazonStats.attributedConversions14d;
            if (cpProgStats is DatedStatsSummaryWithRev)
                ((DatedStatsSummaryWithRev)cpProgStats).PostClickRev = amazonStats.attributedSales14d;
        }
        public static void SetCPProgStats(StatsSummary cpProgStats, IEnumerable<Amazon.Entities.StatSummary> amazonStats)
        {
            bool any = (amazonStats != null && amazonStats.Any());
            if (any)
            {
                cpProgStats.Cost = amazonStats.Sum(x => x.cost);
                cpProgStats.Impressions = amazonStats.Sum(x => x.impressions);
                cpProgStats.Clicks = amazonStats.Sum(x => x.clicks);
                cpProgStats.PostClickConv = amazonStats.Sum(x => x.attributedConversions14d);
                if (cpProgStats is DatedStatsSummaryWithRev)
                    ((DatedStatsSummaryWithRev)cpProgStats).PostClickRev = amazonStats.Sum(x => x.attributedSales14d);
            }
            //note: not setting stats to 0 if !any
        }
    }

    #region Daily
    //The daily extracter will load data based on date range and sum up the total of all campaigns
    public class AmazonDailySummaryExtracter : AmazonApiExtracter<AmazonDailySummary>
    {
        public AmazonDailySummaryExtracter(AmazonUtility amazonUtility, DateRange dateRange, string clientId, string campaignFilter = null, string campaignFilterOut = null)
            : base(amazonUtility, dateRange, clientId, campaignFilter: campaignFilter, campaignFilterOut: campaignFilterOut)
        { }

        protected override void Extract()
        {
            Logger.Info("Extracting DailySummaries from Amazon API for ({0}) from {1:d} to {2:d}",
                this.clientId, this.dateRange.FromDate, this.dateRange.ToDate);

            long[] campaignIds = null;
            if (this.HasCampaignFilter())
            {
                var campaigns = LoadCampaignsFromAmazonAPI(); // only SponsoredProduct campaigns
                campaignIds = campaigns.Select(x => x.campaignId).ToArray();
            }
            foreach (var date in dateRange.Dates)
            {
                var spSums = EnumerateRows(AmazonUtility.CAMPAIGNTYPE_SPONSOREDPRODUCTS, date);
                var hsaSums = EnumerateRows(AmazonUtility.CAMPAIGNTYPE_HSA, date, includeCampaignName: true);
                if (this.HasCampaignFilter())
                {
                    spSums = spSums.Where(x => campaignIds.Contains(x.campaignId));
                    if (!String.IsNullOrEmpty(campaignFilter))
                        hsaSums = hsaSums.Where(x => x.campaignName.Contains(campaignFilter));
                    if (!String.IsNullOrEmpty(campaignFilterOut))
                        hsaSums = hsaSums.Where(x => !x.campaignName.Contains(campaignFilterOut)).ToList();
                }
                var sums = spSums.Concat(hsaSums);
                var dailySum = new AmazonDailySummary
                {
                    date = date,
                };
                dailySum.SetStatTotals(sums);
                Add(dailySum);
            }
            End();
        }
        private IEnumerable<AmazonDailySummary> EnumerateRows(string campaignType, DateTime date, bool includeCampaignName = false)
        {
            var parms = _amazonUtility.CreateAmazonApiReportParams(campaignType, date, includeCampaignName: includeCampaignName);
            var submitReportResponse = _amazonUtility.SubmitReport(parms, "campaigns", this.clientId);
            if (submitReportResponse != null)
            {
                string json = _amazonUtility.WaitForReportAndDownload(submitReportResponse.reportId, this.clientId);
                if (json != null)
                {
                    var dailyStats = JsonConvert.DeserializeObject<List<AmazonDailySummary>>(json);
                    return dailyStats;
                }
            }
            return new List<AmazonDailySummary>();
        }
    }
    #endregion

    #region Campaign/Strategy
    public class AmazonCampaignSummaryExtracter : AmazonApiExtracter<StrategySummary>
    {
        public AmazonCampaignSummaryExtracter(AmazonUtility amazonUtility, DateRange dateRange, string clientId, string campaignFilter = null, string campaignFilterOut = null)
            : base(amazonUtility, dateRange, clientId, campaignFilter: campaignFilter, campaignFilterOut: campaignFilterOut)
        { }

        protected override void Extract()
        {
            Logger.Info("Extracting StrategySummaries from Amazon API for ({0}) from {1:d} to {2:d}", 
                this.clientId, this.dateRange.FromDate, this.dateRange.ToDate);

            var campaigns = LoadCampaignsFromAmazonAPI();
            foreach (var date in dateRange.Dates)
            {
                var items = EnumerateRows(date, campaigns);
                Add(items);
            }
            End();
        }

        //TODO? Request the SP and HSA reports in parallel... ?Okay for two threads to call Add at the same time?
        //TODO? Do multiple dates in parallel

        public IEnumerable<StrategySummary> EnumerateRows(DateTime date, IEnumerable<AmazonCampaign> campaigns)
        {
            //As of v20180312, campaignName is an undocumented metric for the sponsoredProducts report, so we'll hold off using it
            //Also, the get-campaigns call only returns sponsoredProduct campaigns anyway

            var sums = EnumerateReport(AmazonUtility.CAMPAIGNTYPE_SPONSOREDPRODUCTS, date, campaigns);
            foreach (var sum in sums)
                yield return sum;
            var hsaSums = EnumerateReport(AmazonUtility.CAMPAIGNTYPE_HSA, date); // don't pass in campaigns; instead use campaignName metric
            foreach (var sum in hsaSums)
                yield return sum;
        }
        public IEnumerable<StrategySummary> EnumerateReport(string campaignType, DateTime date, IEnumerable<AmazonCampaign> campaigns = null)
        {
            var parms = _amazonUtility.CreateAmazonApiReportParams(campaignType, date, includeCampaignName: (campaigns == null));
            var submitReportResponse = _amazonUtility.SubmitReport(parms, "campaigns", this.clientId);
            if (submitReportResponse != null)
            {
                string json = _amazonUtility.WaitForReportAndDownload(submitReportResponse.reportId, this.clientId);
                if (json != null)
                {
                    var dailyStats = JsonConvert.DeserializeObject<List<AmazonDailySummary>>(json);
                    foreach (var sum in GroupAndEnumerate(dailyStats, date, campaigns))
                        yield return sum;
                }
            }
        }
        private IEnumerable<StrategySummary> GroupAndEnumerate(List<AmazonDailySummary> dailyStats, DateTime date, IEnumerable<AmazonCampaign> campaigns = null)
        {
            dailyStats = dailyStats.Where(x => !x.AllZeros()).ToList();
            if (campaigns == null)
            {   // using campaignName metric; filter on that (if there's a filter)
                if (!String.IsNullOrEmpty(campaignFilter))
                    dailyStats = dailyStats.Where(x => x.campaignName.Contains(campaignFilter)).ToList();
                if (!String.IsNullOrEmpty(campaignFilterOut))
                    dailyStats = dailyStats.Where(x => !x.campaignName.Contains(campaignFilterOut)).ToList();

                foreach (var ds in dailyStats)
                {
                    var sum = new StrategySummary
                    {
                        Date = date,
                        StrategyEid = ds.campaignId.ToString(),
                        StrategyName = ds.campaignName
                    };
                    SetCPProgStats(sum, ds);
                    yield return sum;
                }
            }
            else foreach (var campaign in campaigns)
            {
                var statsGroup = dailyStats.Where(x => x.campaignId == campaign.campaignId);
                if (statsGroup.Any())
                {
                    var sum = new StrategySummary // most likely there's just one dailyStat in the group, but this covers everything...
                    {
                        Date = date,
                        StrategyEid = campaign.campaignId.ToString(),
                        StrategyName = campaign.name
                    };
                    SetCPProgStats(sum, statsGroup);
                    yield return sum;
                }
            }
        }
    }
    #endregion

    #region AdSet (Keyword)
    public class AmazonAdSetExtracter : AmazonApiExtracter<AdSetSummary>
    {
        public AmazonAdSetExtracter(AmazonUtility amazonUtility, DateRange dateRange, string clientId, string campaignFilter = null, string campaignFilterOut = null)
            : base(amazonUtility, dateRange, clientId, campaignFilter: campaignFilter, campaignFilterOut: campaignFilterOut)
        { }

        //Note: The API only returns keywords for sponsoredProduct campaigns, and they are at the adgroup level.  So presumably two Keyword objects
        // could have the same text but be for two different adgroups in the same campaign.

        //TODO: Instead of keyword stats, get adgroup level stats here.  If needed, establish a new stats level in the db: Keyword, KeywordSummary
        // (where a Keyword is actually a set of keywords)

        protected override void Extract()
        {            
            Logger.Info("Extracting AdSetSummaries from Amazon API for ({0}) from {1:d} to {2:d}",
                this.clientId, this.dateRange.FromDate, this.dateRange.ToDate);

            var campaigns = LoadCampaignsFromAmazonAPI();
            var campIds = campaigns.Select(x => x.campaignId).ToArray();
            List<AmazonAdSet> adsets = LoadAdSetsfromAmazonAPI(campaignIds: campIds);

            if (adsets != null)
            {
                foreach (var date in dateRange.Dates)
                {
                    var items = EnumerateRows(date, adsets);
                    Add(items);
                }
            }
            End();
        }

        private List<AmazonAdSet> LoadAdSetsfromAmazonAPI(long[] campaignIds = null)
        {
            List<AmazonAdSet> adsets = new List<AmazonAdSet>();
            var keywords = _amazonUtility.GetKeywords(clientId);

            if (keywords == null) return null;

            foreach (var keyword in keywords)
            {
                if (campaignIds == null || campaignIds.Contains(keyword.CampaignId))
                {
                    adsets.Add(new AmazonAdSet
                    {
                        KeywordText = keyword.KeywordText,
                        CampaignId = keyword.CampaignId.ToString(),
                        KeywordId = keyword.KeywordId.ToString()
                    });
                }
            }
            return adsets;
        }
        //private List<string> LoadDistinctKeywordsfromAmazonAPI()
        //{
        //    var keywords = _amazonUtility.GetKeywords(clientId);
        //    if (keywords == null) return null;

        //    var uniqueKeywords = keywords.Select(x => x.KeywordText).Distinct().ToList();
        //    return uniqueKeywords;
        //}

        private IEnumerable<AdSetSummary> EnumerateRows(DateTime date, List<AmazonAdSet> adsets)
        {
            var parms = _amazonUtility.CreateAmazonApiReportParams(AmazonUtility.CAMPAIGNTYPE_SPONSOREDPRODUCTS, date);
            var submitReportResponse = _amazonUtility.SubmitReport(parms, "keywords", this.clientId);
            if (submitReportResponse != null)
            {
                string json = _amazonUtility.WaitForReportAndDownload(submitReportResponse.reportId, this.clientId);
                if (json != null)
                {
                    var dailyStats = JsonConvert.DeserializeObject<List<AmazonKeywordDailySummary>>(json);
                    foreach (var sum in GroupAndEnumerate(dailyStats, date, adsets))
                        yield return sum;
                }
            }
        }
        private IEnumerable<AdSetSummary> GroupAndEnumerate(List<AmazonKeywordDailySummary> dailyStats, DateTime date, IEnumerable<AmazonAdSet> adsets)
        {
            var keywordGroups = adsets.GroupBy(x => x.KeywordText); //TODO: do this outside of loop-by-day (above)

            foreach (var keywordGroup in keywordGroups)
            {
                var keywordIds = keywordGroup.Select(x => x.KeywordId).ToArray();
                var statsGroup = dailyStats.Where(x => keywordIds.Contains(x.KeywordId) && !x.AllZeros());
                if (statsGroup.Any())
                {
                    var sum = new AdSetSummary
                    {
                        Date = date,
                        AdSetName = keywordGroup.Key
                    };
                    SetCPProgStats(sum, statsGroup);
                    yield return sum;
                }
            }
        }

    }
    #endregion

    #region Ad (ProductAd)
    public class AmazonAdExtrater : AmazonApiExtracter<TDadSummary>
    {
        //NOTE: We can only get ad stats for SponsoredProduct campaigns, for these reasons:
        // - the get-ProductAds call only returns SP ads
        // - for HSA reports, recordType call only be campaigns, adGroups or keywords
        // - a productAdId metric is not available anyway
        // (as of v.20180314)

        public AmazonAdExtrater(AmazonUtility amazonUtility, DateRange dateRange, string clientId, string campaignFilter = null, string campaignFilterOut = null)
            : base(amazonUtility, dateRange, clientId, campaignFilter: campaignFilter, campaignFilterOut: campaignFilterOut)
        { }

        protected override void Extract()
        {
            Logger.Info("Extracting TDadSummaries from Amazon API for ({0}) from {1:d} to {2:d}",
                this.clientId, this.dateRange.FromDate, this.dateRange.ToDate);

            // This didn't work. The stats (e.g. spend) were larger than what we got at the campaign/keyword levels.
            var campaigns = LoadCampaignsFromAmazonAPI();
            var campIds = campaigns.Select(x => x.campaignId).ToArray();
            List<TDad> ads = LoadAdsFromAmazonAPI(campaignIds: campIds);

            //List<TDad> ads = LoadAdsFromAmazonAPI();

            if (ads != null)
            {
                foreach (var date in dateRange.Dates)
                {
                    var items = EnumerateRows(date, ads);
                    Add(items);
                }
            }
            End();
        }

        //NOTE: This only retrieves SponsoredProduct ads
        private List<TDad> LoadAdsFromAmazonAPI(long[] campaignIds = null)
        {
            List<TDad> ads = new List<TDad>();
            var productAds = _amazonUtility.GetProductAds(clientId);
            if (productAds == null) return null;
            if (campaignIds != null)
                productAds = productAds.Where(x => campaignIds.Contains(x.CampaignId)).ToList();

            foreach (var productAdGroup in productAds.GroupBy(x => x.AdId))
            {
                var ad = new TDad
                {
                    ExternalId = productAdGroup.Key,
                    Name = productAdGroup.First().Asin
                };
                if (productAdGroup.Count() > 1)
                {
                    Logger.Info("Multiple ads for {0}", productAdGroup.Key);
                    var names = productAdGroup.Select(x => x.Asin).ToArray();
                    ad.Name = String.Join(",", names);
                }
                ads.Add(ad);
            }
            return ads;
        }
        //private List<string> LoadDistinctAdNamesFromAmazonAPI()
        //{
        //    var productAds = _amazonUtility.GetProductAds(clientId);
        //    if (productAds == null) return null;

        //    var uniqueAdNames = productAds.Select(x => x.Asin).Distinct().ToList();
        //    return uniqueAdNames;
        //}

        private IEnumerable<TDadSummary> EnumerateRows(DateTime date, List<TDad> productAds)
        {
            var parms = _amazonUtility.CreateAmazonApiReportParams(AmazonUtility.CAMPAIGNTYPE_SPONSOREDPRODUCTS, date);
            var submitReportResponse = _amazonUtility.SubmitReport(parms, "productAds", this.clientId);
            if (submitReportResponse != null)
            {
                string json = _amazonUtility.WaitForReportAndDownload(submitReportResponse.reportId, this.clientId);
                if (json != null)
                {
                    var dailyStats = JsonConvert.DeserializeObject<List<AmazonAdDailySummary>>(json);
                    foreach (var sum in GroupAndEnumerate(dailyStats, date, productAds))
                        yield return sum;
                }
            }
        }
        private IEnumerable<TDadSummary> GroupAndEnumerate(List<AmazonAdDailySummary> productAdsDailyStats, DateTime date, IEnumerable<TDad> productAds)
        {
            var adNameGroups = productAds.GroupBy(x => x.Name); //TODO: do this outside of loop-by-day (above)

            foreach (var adNameGroup in adNameGroups)
            {
                var adIds = adNameGroup.Select(x => x.ExternalId).ToArray();
                var statsGroup = productAdsDailyStats.Where(x => adIds.Contains(x.adId) && !x.AllZeros());
                if (statsGroup.Any())
                {
                    var sum = new TDadSummary
                    {
                        Date = date,
                        //TDadEid =
                        TDadName = adNameGroup.Key,
                    };
                    SetCPProgStats(sum, statsGroup);
                    yield return sum;
                }
            }
        }
    }
    #endregion

}
