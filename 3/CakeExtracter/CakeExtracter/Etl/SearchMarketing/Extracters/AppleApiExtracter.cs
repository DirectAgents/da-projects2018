using System;
using Apple;
using CakeExtracter.Common;

namespace CakeExtracter.Etl.SearchMarketing.Extracters
{
    public class AppleApiExtracter : Extracter<AppleStatGroup>
    {
        protected readonly AppleAdsUtility appleAdsUtility;
        protected readonly DateRange dateRange;
        protected readonly string orgId;
        protected readonly string certificateCode;

        public AppleApiExtracter(AppleAdsUtility appleAdsUtility, DateRange dateRange, string orgId, string certificateCode)
        {
            this.appleAdsUtility = appleAdsUtility;
            this.dateRange = dateRange;
            this.orgId = orgId;
            this.certificateCode = certificateCode;
        }

        //TODO: Handle gaps in the stats. Somehow pass deletion items... how to do this for all campaigns?

        protected override void Extract()
        {
            Logger.Info("Extracting daily stats from AppleAds API for ({0}) from {1:d} to {2:d}",
                orgId, dateRange.FromDate, dateRange.ToDate);
            try
            {
                var start = dateRange.FromDate;
                var end = dateRange.ToDate;
                var date90daysAfterStart = start.AddDays(90);
                if (end > date90daysAfterStart)
                    end = date90daysAfterStart;
                while (start <= dateRange.ToDate)
                {
                    var appleStatGroups = appleAdsUtility.GetCampaignDailyStats(start, end, orgId, certificateCode);
                    if (appleStatGroups != null)
                        Add(appleStatGroups);
                    start = start.AddDays(91);
                    end = end.AddDays(91);
                    if (end > dateRange.ToDate)
                        end = dateRange.ToDate;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            End();
        }
    }
}
