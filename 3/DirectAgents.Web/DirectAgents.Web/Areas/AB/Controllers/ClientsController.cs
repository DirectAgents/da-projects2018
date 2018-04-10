using System;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Web.Areas.AB.Controllers
{
    public class ClientsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public ClientsController(IABRepository abRepository)
        {
            this.abRepo = abRepository;
        }

        public ActionResult Index()
        {
            var clients = abRepo.Clients();
            return View(clients);
        }

        public ActionResult Show(int id)
        {
            var abClient = abRepo.Client(id);
            if (abClient == null)
                return HttpNotFound();

            return View(abClient);
        }
        public ActionResult ShowX(int id) // initial version
        {
            var abClient = abRepo.Client(id);
            if (abClient == null)
                return HttpNotFound();

            return View(abClient);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var abClient = abRepo.Client(id);
            if (abClient == null)
                return HttpNotFound();

            return View(abClient);
        }
        [HttpPost]
        public ActionResult Edit(ABClient client)
        {
            if (ModelState.IsValid)
            {
                if (abRepo.SaveClient(client))
                    return RedirectToAction("Show", new { id = client.Id });
                ModelState.AddModelError("", "Client could not be saved.");
            }
            return View(client);
        }

        public ActionResult NewBudget(int id)
        {
            var abClient = abRepo.Client(id);
            if (abClient == null)
                return HttpNotFound();

            if (!abClient.ClientBudgets.Any()) // why this check?
            {
                var clientBudget = new ClientBudget
                {
                    Date = DateTime.Today.FirstDayOfMonth()
                };
                abClient.ClientBudgets.Add(clientBudget);
                abRepo.SaveChanges();
            }
            return RedirectToAction("Show", new { id = abClient.Id });
        }

	}
}