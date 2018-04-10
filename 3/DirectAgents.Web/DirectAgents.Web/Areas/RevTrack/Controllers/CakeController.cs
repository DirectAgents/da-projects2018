using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Web.Areas.RevTrack.Controllers
{
    public class CakeController : DirectAgents.Web.Controllers.ControllerBase
    {
        public CakeController(IMainRepository daRepository, IABRepository abRepository)
        {
            this.daRepo = daRepository;
            this.abRepo = abRepository;
        }

        public ActionResult Advertisers()
        {
            var advs = daRepo.GetAdvertisers();

            return View(advs);
        }

        public ActionResult CheckCreateABClients()
        {
            var cake_DeptRepo = daRepo.Create_Cake_DeptRepository();

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var rtLineItems = cake_DeptRepo.StatsByClient(monthStart);
            var advIds = rtLineItems.Select(li => li.RTId).ToArray();

            var abClients = abRepo.Clients();
            int abClientId = abClients.Any() ? abClients.Max(c => c.Id) + 1 : 1;

            var clientsToAdd = new List<ABClient>();
            var advs = daRepo.GetAdvertisers().Where(a => advIds.Contains(a.AdvertiserId));
            foreach (var adv in advs.OrderBy(a => a.AdvertiserName))
            {
                adv.ABClientId = abClientId;
                clientsToAdd.Add(new ABClient
                {
                    Id = abClientId++,
                    Name = adv.AdvertiserName
                });
            }
            abRepo.AddClients(clientsToAdd);

            daRepo.SaveChanges(); // update ABClientId values

            return Content(rtLineItems.Count().ToString());
        }
	}
}