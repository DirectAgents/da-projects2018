using Criteo.CriteoAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;

namespace Criteo
{
    public class CriteoUtility
    {
        private readonly string _password = ConfigurationManager.AppSettings["CriteoPassword"];
        private readonly string _source = ConfigurationManager.AppSettings["CriteoSource"];

        private string Username { get; set; }
        private long AppToken { get; set; }

        public void SetCredentials(string account)
        {
            this.Username = null;
            this.AppToken = 0;

            string username = ConfigurationManager.AppSettings["CriteoUsername_" + account];
            if (!String.IsNullOrWhiteSpace(username))
                this.Username = username;

            string appToken = ConfigurationManager.AppSettings["CriteoAppToken_" + account];
            if (!String.IsNullOrWhiteSpace(appToken))
                this.AppToken = Convert.ToInt64(appToken);
        }

        private CriteoAdvertiserAPISoapClient _service;
        private apiHeader _apiHeader;

        // --- Logging ---
        private Action<string> _LogInfo;
        private Action<string> _LogError;

        private void LogInfo(string message)
        {
            if (_LogInfo == null)
                Console.WriteLine(message);
            else
                _LogInfo("[CriteoUtility] " + message);
        }

        private void LogError(string message)
        {
            if (_LogError == null)
                Console.WriteLine(message);
            else
                _LogError("[CriteoUtility] " + message);
        }

        // --- Constructors ---
        public CriteoUtility()
        {
        }
        public CriteoUtility(Action<string> logInfo, Action<string> logError)
        {
            _LogInfo = logInfo;
            _LogError = logError;
        }

        // --- public methods ---

        public campaign[] GetCampaigns(int? campaignID = null)
        {
            var campaignIDs = new List<int>();
            if (campaignID.HasValue)
                campaignIDs.Add(campaignID.Value);
            return GetCampaigns(campaignIDs.ToArray());
        }
        public campaign[] GetCampaigns(int[] campaignIDs)
        {
            campaign[] campaigns = null;
            bool closed = false;
            try
            {
                StartServiceAndLogin();

                var sel = new CampaignSelectors()
                {
                    campaignIDs = campaignIDs,
                    //campaignStatus = new[] { CampaignStatus.RUNNING }
                };
                var x = _service.getAccount(_apiHeader);
                campaigns = _service.getCampaigns(_apiHeader, sel);

                _service.Close();
                closed = true;
            }
            catch (Exception e)
            {
                LogError(e.Message);
            }
            finally
            {
                if (!closed && _service != null)
                    _service.Abort();
            }
            return campaigns;
        }

        // returns url of report
        public string GetCampaignReport(DateTime start, DateTime end, bool hourly = false)
        {
            string url = null;
            bool closed = false;
            try
            {
                StartServiceAndLogin();
                url = GetCampaignReportInner(start, end, hourly);

                _service.Close();
                closed = true;
            }
            catch (Exception e)
            {
                LogError(e.Message);
            }
            finally
            {
                if (!closed && _service != null)
                    _service.Abort();
            }
            return url;
        }


        // --- private methods ---
        private void StartServiceAndLogin()
        {
            _service = new CriteoAdvertiserAPISoapClient();
            var authToken = _service.clientLogin(this.Username, _password, _source);

            _apiHeader = new apiHeader()
            {
                authToken = authToken,
                appToken = this.AppToken,
                //clientVersion = "1"
            };
        }

        // returns url of report
        private string GetCampaignReportInner(DateTime start, DateTime end, bool hourly)
        {
            LogInfo(String.Format("Generating {0} campaign report for {1:d} to {2:d}",
                (hourly ? "hourly" : "daily"), start, end));

            var reportJob = new ReportJob()
            {
                //reportSelector =
                reportType = ReportType.Campaign,
                aggregationType = (hourly ? AggregationType.Hourly : AggregationType.Daily),
                startDate = start.ToString("yyyy-MM-dd"),
                endDate = end.ToString("yyyy-MM-dd"),
                selectedColumns = new ReportColumn[] { ReportColumn.clicks, ReportColumn.cost, ReportColumn.impressions, ReportColumn.orderValue, ReportColumn.sales }
            };
            var resp = _service.scheduleReportJob(_apiHeader, reportJob);
            // resp.jobStatus should be "pending"

            var jobStatus = _service.getJobStatus(_apiHeader, resp.jobID);
            var waitTime = new TimeSpan(0, 0, 3);
            while (jobStatus != JobStatus.Completed)
            {
                Thread.Sleep(waitTime);
                jobStatus = _service.getJobStatus(_apiHeader, resp.jobID);
            }
            var url = _service.getReportDownloadUrl(_apiHeader, resp.jobID);
            return url;
        }

    }
}
