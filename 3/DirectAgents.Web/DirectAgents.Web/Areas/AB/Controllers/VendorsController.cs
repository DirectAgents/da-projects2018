using System;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Web.Areas.AB.Controllers
{
    public class VendorsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public VendorsController(IABRepository abRepository)
        {
            this.abRepo = abRepository;
        }

        public ActionResult Index()
        {
            var vendors = abRepo.Vendors();
            return View(vendors);
        }
	}
}