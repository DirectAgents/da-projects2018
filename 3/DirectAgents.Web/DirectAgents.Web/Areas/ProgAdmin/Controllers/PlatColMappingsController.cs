using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class PlatColMappingsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public PlatColMappingsController(ICPProgRepository cpProgRepository)
        {
            this.cpProgRepo = cpProgRepository;
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var platColMapping = cpProgRepo.PlatColMapping(id);
            if (platColMapping == null)
            {
                var platform = cpProgRepo.Platform(id);
                if (platform == null)
                    return HttpNotFound();

                platColMapping = new PlatColMapping
                {
                    Id = id,
                    Platform = platform
                };
                platColMapping.SetDefaults();
            }
            return View(platColMapping);
        }
        [HttpPost]
        public ActionResult Edit(PlatColMapping mapping)
        {
            if (ModelState.IsValid)
            {
                if (cpProgRepo.AddSavePlatColMapping(mapping))
                    return RedirectToAction("Edit", "Platforms", new { id = mapping.Id });
                ModelState.AddModelError("", "PlatColMapping could not be saved.");
            }
            cpProgRepo.FillExtended(mapping);
            return View(mapping);
        }
	}
}