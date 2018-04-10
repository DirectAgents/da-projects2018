using System;
using System.Collections.Generic;
using AdRoll.Entities;

namespace AdRoll.Clients
{
    public abstract class ReportRequest : ApiRequest
    {
        public string campaigns { get; set; }
        public string external_campaigns { get; set; }
        public string adgroups { get; set; }
        public string ads { get; set; }
        public string advertisables { get; set; }

        public string data_format { get; set; }

        //public int? past_days { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }

        public string advertisable_eid { get; set; }
        public string breakdowns { get; set; }

        public ReportRequest()
        {
            start_date = DateTime.Today.AddDays(-1).ToString("MM-dd-yyyy");
            end_date = start_date;
        }
    }

    #region Advertisable

    public class AdvertisableReportClient : ApiClient
    {
        public AdvertisableReportClient()
            : base(1, "report", "advertisable") { }

        public DailySummaryReportResponse DailySummaries(AdvertisableReportRequest request)
        {
            var result = Execute<DailySummaryReportResponse>(request);
            return result;
        }
        public EntityReportResponse Summaries(AdvertisableReportRequest request)
        {
            var result = Execute<EntityReportResponse>(request);
            return result;
        }
    }

    public class AdvertisableReportRequest : ReportRequest
    {
        public AdvertisableReportRequest()
            : base()
        {
            data_format = "date";
        }
    }
    public class DailySummaryReportResponse
    {
        public List<AdrollDailySummary> results { get; set; }
    }
    public class EntityReportResponse
    {
        public List<AdvertisableSummary> results { get; set; }
    }

    #endregion
    #region Campaign

    public class CampaignReportClient : ApiClient
    {
        public CampaignReportClient()
            : base(1, "report", "campaign") { }

        public CampaignSummaryReportResponse CampaignSummaries(CampaignReportRequest request)
        {
            var result = Execute<CampaignSummaryReportResponse>(request);
            return result;
        }
    }

    public class CampaignReportRequest : ReportRequest
    {
        public CampaignReportRequest()
            : base()
        {
            data_format = "entity";
        }
    }
    public class CampaignSummaryReportResponse
    {
        public List<CampaignSummary> results { get; set; }
    }

    #endregion
    #region Ad

    public class AdReportClient : ApiClient
    {
        public AdReportClient()
            : base(1, "report", "ad") { }

        public AdSummaryReportResponse AdSummaries(AdReportRequest request)
        {
            var result = Execute<AdSummaryReportResponse>(request);
            return result;
        }
    }

    public class AdReportRequest : ReportRequest
    {
        public AdReportRequest()
            : base()
        {
            data_format = "entity";
        }
    }
    public class AdSummaryReportResponse
    {
        public List<AdSummary> results { get; set; }
    }

    #endregion
    #region Attribution

    public class AttributionReportClient : ApiClient
    {
        public AttributionReportClient()
            : base(1, "attributions", "advertisable", reporting: true) { }

        public AttributionReportResponse DailySummaries(AttributionReportRequest request)
        {
            var result = Execute<AttributionReportResponse>(request);
            return result;
        }
    }

    public class AttributionReportRequest : ReportRequest
    {
        public AttributionReportRequest()
            : base()
        {
            breakdowns = "date";
        }
    }
    public class AttributionReportResponse
    {
        public AttributionReportResponseInner results { get; set; }
    }
    public class AttributionReportResponseInner
    {
        public List<AttributionSummary> date { get; set; }
        // entity
        // summary
    }

    #endregion
}
