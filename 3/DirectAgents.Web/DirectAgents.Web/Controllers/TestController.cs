using System;
using System.Web.Mvc;
using CakeExtracter.Commands;

namespace DirectAgents.Web.Controllers
{
    public class TestController : ControllerBase
    {
        public ActionResult SynchSearch()
        {
            int searchProfileId = 40;
            DateTime? start = new DateTime(2015, 11, 1);
            DateTime? end = null;
            int? daysAgoToStart = null;
            string getClickAssistConvStats = "both";
            //SynchSearchDailySummariesAdWordsCommand.RunStatic(searchProfileId, start, end, daysAgoToStart, getClickAssistConvStats);
            //SynchSearchDailySummariesBingCommand.RunStatic(searchProfileId, start, end, daysAgoToStart);

            return Content("okay");
        }
	}
}