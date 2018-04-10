using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Web.Areas.AB.Controllers
{
    public class PaymentsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public PaymentsController(IABRepository abRepository)
        {
            this.abRepo = abRepository;
        }

        public ActionResult Index(int clientId)
        {
            var abClient = abRepo.Client(clientId);
            if (abClient == null)
                return HttpNotFound();

            return View(abClient);
        }

        public ActionResult New(int clientId)
        {
            var abClient = abRepo.Client(clientId);
            if (abClient == null || abClient.ClientAccounts == null || !abClient.ClientAccounts.Any())
                return HttpNotFound();
            var firstAccount = abClient.ClientAccounts.OrderBy(x => x.Id).First();

            var bit = new PaymentBit { AcctId = firstAccount.Id };
            var payment = new Payment
            {
                Date = DateTime.Today,
                Bits = new Collection<PaymentBit>() { bit }
            };
            abClient.Payments.Add(payment);
            abRepo.SaveChanges();

            return RedirectToAction("Index", new { clientId = abClient.Id });
        }

        //TODO: Edit (like in ClientPaymentsController)
    }
}