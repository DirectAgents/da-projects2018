using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Web.Areas.AB.Controllers
{
    public class JobsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public JobsController(IABRepository abRepository)
        {
            this.abRepo = abRepository;
        }

        public ActionResult Index(int clientId, int? editJobId)
        {
            var abClient = abRepo.Client(clientId);
            if (abClient == null)
                return HttpNotFound();

            ViewBag.EditJobId = editJobId;
            return View(abClient);
        }

        public ActionResult JobRow(int id, bool? editMode)
        {
            var job = abRepo.Job(id);
            if (job == null)
                return null;

            ViewBag.EditMode = editMode;
            return PartialView(job);
        }
        [HttpPost]
        public JsonResult SaveJobRow(Job inJob)
        {
            bool saved = false;
            if (ModelState.IsValid)
                saved = SaveJob(inJob);
            return Json(saved);
        }
        private bool SaveJob(Job inJob)
        {
            var job = abRepo.Job(inJob.Id);
            if (job == null)
                return false;
            job.Name = inJob.Name;
            abRepo.SaveChanges();
            return true;
            //TODO: catch exceptions and return false?
        }

        public ActionResult New(int clientId)
        {
            var abClient = abRepo.Client(clientId);
            if (abClient == null)
                return HttpNotFound();

            var job = new Job
            {
                Name = "zNew"
            };
            abClient.Jobs.Add(job);
            abRepo.SaveChanges();

            return RedirectToAction("Index", new { clientId = abClient.Id, editJobId = job.Id });
        }

        public ActionResult Delete(int id)
        {
            var job = abRepo.Job(id);
            if (job == null)
                return HttpNotFound();

            int clientId = job.ClientId;
            abRepo.DeleteJob(job);

            return RedirectToAction("Index", new { clientId = clientId });
        }

    }
}