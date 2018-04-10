using System;

namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class TrafficRequest : ApiRequest
    {
        public TrafficRequest()
        {
            start_date = DateTime.Today.ToString("MM/dd/yyyy");
            end_date = DateTime.Today.AddDays(1).ToString("MM/dd/yyyy");
        }

        //start_date / DATETIME = Report Start Date [MM/DD/YYYY HH:MM:SS]
        public string start_date { get; set; }

        //end_date / DATETIME = Report End Date [MM/DD/YYYY HH:MM:SS]
        public string end_date { get; set; }
    }
}