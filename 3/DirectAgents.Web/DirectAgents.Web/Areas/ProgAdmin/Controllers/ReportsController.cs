using System;
using System.Configuration;
using System.Net;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using DirectAgents.Domain.Abstract;
using Microsoft.Reporting.WebForms;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class ReportsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public ReportsController(ICPProgRepository cpProgRepository)
        {
            this.cpProgRepo = cpProgRepository;
        }

        public ActionResult Home(int campId)
        {
            var campaign = cpProgRepo.Campaign(campId);
            if (campaign == null)
                return HttpNotFound();
            return View(campaign);
        }

        public ActionResult ReportViewer()
        {
            ReportViewer rv = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Remote,
                SizeToReportContent = true,
                Width = Unit.Percentage(100),
                Height = Unit.Percentage(100)
            };

            rv.ServerReport.ReportPath = "/DA - Trading Desk Report";
            rv.ServerReport.ReportServerUrl = new Uri(ConfigurationManager.AppSettings["SSRS_ReportServerUrl"]);

            string username = ConfigurationManager.AppSettings["SSRS_ReportServerUsername"];
            string password = ConfigurationManager.AppSettings["SSRS_ReportServerPassword"];
            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
                rv.ServerReport.ReportServerCredentials = new ReportServerCredentials(username, password, "");

            ViewBag.ReportViewer = rv;
            return View();
        }
	}

    public class ReportServerCredentials : IReportServerCredentials
    {
        private string _userName;
        private string _password;
        private string _domain;

        public ReportServerCredentials(string userName, string password, string domain)
        {
            _userName = userName;
            _password = password;
            _domain = domain;
        }

        public WindowsIdentity ImpersonationUser
        {
            get { return null; }
        }
        public ICredentials NetworkCredentials
        {
            get { return new NetworkCredential(_userName, _password, _domain); }
        }
        public bool GetFormsCredentials(out Cookie authCookie, out string user, out string password, out string authority)
        {
            // Do not use forms credentials to authenticate.
            authCookie = null;
            user = password = authority = null;
            return false;
        }
    }
}