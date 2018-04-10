using System;
using System.Linq;
using System.Web.Mvc;
using CakeExtracter.Commands;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class PlatformsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public PlatformsController(ICPProgRepository cpProgRepository)
        {
            this.cpProgRepo = cpProgRepository;
        }

        public ActionResult Index()
        {
            var platforms = cpProgRepo.Platforms()
                .OrderBy(p => p.Name);
            return View(platforms);
        }

        public ActionResult CreateNew()
        {
            var platform = new Platform
            {
                Code = "z",
                Name = "zNew"
            };
            if (cpProgRepo.AddPlatform(platform))
                return RedirectToAction("Index");
            else
                return Content("Error creating Partner");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var platform = cpProgRepo.Platform(id);
            if (platform == null)
                return HttpNotFound();
            return View(platform);
        }
        [HttpPost]
        public ActionResult Edit(Platform plat)
        {
            if (ModelState.IsValid)
            {
                if (cpProgRepo.SavePlatform(plat, includeTokens: false))
                    return RedirectToAction("Index");
                ModelState.AddModelError("", "Platform could not be saved.");
            }
            // ?fillextended / setupforedit?
            return View(plat);
        }

        public ActionResult Maintenance(int id)
        {
            var platform = cpProgRepo.Platform(id);
            if (platform == null)
                return HttpNotFound();

            return View(platform);
        }

        public ActionResult SyncAccounts(int id)
        {
            var platform = cpProgRepo.Platform(id);
            if (platform == null)
                return HttpNotFound();
            if (platform.Code == Platform.Code_AdRoll)
            {
                DASynchAdrollAccounts.RunStatic();
            }
            return RedirectToAction("Maintenance", new { id = id });
        }

        public ActionResult SyncStats(int id, DateTime? start)
        {
            var platform = cpProgRepo.Platform(id);
            if (platform == null)
                return HttpNotFound();

            if (platform.Code == Platform.Code_Adform)
                DASynchAdformStats.RunStatic(startDate: start);
            else if (platform.Code == Platform.Code_AdRoll)
                DASynchAdrollStats.RunStatic(startDate: start, oneStatPer: "all");
            else if (platform.Code == Platform.Code_DBM)
                DASynchDBMStats.RunStatic(startDate: start);
            else if (platform.Code == Platform.Code_FB)
                DASynchFacebookStats.RunStatic(startDate: start);
            else if (platform.Code == Platform.Code_YAM)
                DASynchYAMStats.RunStatic(startDate: start);

            return RedirectToAction("Maintenance", new { id = id });
        }
	}
}