using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using ClientPortal.Data.Contracts;
using DAGenerators.Spreadsheets;

namespace DirectAgents.Web.Areas.SearchAdmin.Controllers
{
    public class OldController : DirectAgents.Web.Controllers.ControllerBase
    {
        public OldController(IClientPortalRepository cpRepository)
        {
            this.cpRepo = cpRepository;
        }

        //public ActionResult Test()
        //{
        //    var sp = cpRepo.GetSearchProfile(78);
        //    return Content("testtest: " + sp.SearchProfileName);
        //}

        [HttpGet]
        public ActionResult GenerateSpreadsheet(int spId)
        {
            var searchProfile = cpRepo.GetSearchProfile(spId);
            if (searchProfile == null)
                return HttpNotFound();

            return View(searchProfile);
        }
        [HttpPost]
        public ActionResult GenerateSpreadsheet(int searchProfileId, DateTime? endDate, int numWeeks = 0, int numMonths = 0, string filename = "report.xlsx", bool groupBySearchAccount = false, string campaignInclude = null, string campaignExclude = null)
        {
            string templateFolder = ConfigurationManager.AppSettings["PATH_Search"];
            if (!endDate.HasValue)
                endDate = DateTime.Today.AddDays(-1); // if not specified; (user can always set endDate to today if desired)

            var spreadsheet = DAGenerators.Spreadsheets.GeneratorCP.GenerateSearchReport(cpRepo, templateFolder, searchProfileId, numWeeks, numMonths, endDate.Value, groupBySearchAccount, campaignInclude, campaignExclude);
            if (spreadsheet == null)
                return HttpNotFound();

            var fsr = new FileStreamResult(spreadsheet.GetAsMemoryStream(), SpreadsheetBase.ContentType);
            fsr.FileDownloadName = filename;
            return fsr;
            //spreadsheet.DisposeResources();
        }

    }
}