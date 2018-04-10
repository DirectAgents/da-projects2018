using System;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class MainController : DirectAgents.Web.Controllers.ControllerBase
    {
        //public MainController(ITDRepository tdRepository)
        //{
        //    this.tdRepo = tdRepository;
        //}

        public ActionResult ChooseMonth(DateTime month)
        {
            SetCurrentMonth("RT", month);
            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult Links()
        {
            return View();
        }
	}
}