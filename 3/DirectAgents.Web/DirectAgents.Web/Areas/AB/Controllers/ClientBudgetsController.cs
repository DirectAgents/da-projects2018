using System;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Web.Areas.AB.Controllers
{
    public class ClientBudgetsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public ClientBudgetsController(IABRepository abRepository)
        {
            this.abRepo = abRepository;
        }

        public ActionResult EditViaLink(int clientId, DateTime date, decimal value)
        {
            var clientBudget = abRepo.ClientBudget(clientId, date);
            if (clientBudget == null)
                return HttpNotFound();

            clientBudget.Value = value;
            abRepo.SaveChanges();
            return RedirectToAction("Show", "Clients", new { id = clientBudget.ClientId });
        }
    }
}