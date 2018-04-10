using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Web.Areas.AB.Controllers
{
    public class ClientPaymentsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public ClientPaymentsController(IABRepository abRepository)
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
            if (abClient == null)
                return HttpNotFound();

            var clientPayment = new ClientPayment
            {
                Date = DateTime.Today,
                Bits = new Collection<ClientPaymentBit>() { new ClientPaymentBit() }
            };
            abClient.ClientPayments.Add(clientPayment);
            abRepo.SaveChanges();

            return RedirectToAction("Index", new { clientId = abClient.Id });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var payment = abRepo.ClientPayment(id);
            if (payment == null)
                return HttpNotFound();

            SetupForEdit(payment);
            return View(payment);
        }
        [HttpPost]
        public ActionResult Edit(ClientPayment payment)
        {
            if (ModelState.IsValid)
            {
                if (abRepo.SaveClientPayment(payment))
                    return RedirectToAction("Edit", new { id = payment.Id });
                ModelState.AddModelError("", "Client could not be saved.");
            }
            abRepo.FillExtended(payment);
            SetupForEdit(payment);
            return View(payment);
        }
        private void SetupForEdit(ClientPayment payment)
        {
            ViewBag.Jobs = abRepo.Jobs(clientId: payment.ClientId);
        }
        [HttpPost]
        public ActionResult SaveBits(int numBits, int paymentId)
        {
            for (int i = 0; i < numBits; i++)
            {
                string idString = Request.Form["id" + i];
                string valString = Request.Form["val" + i];
                string jobIdString = Request.Form["job" + i];
                int id, jobId;
                decimal val;
                if (int.TryParse(idString, out id))
                {
                    var bit = abRepo.ClientPaymentBit(id);
                    if (bit != null)
                    {
                        if (decimal.TryParse(valString, out val))
                            bit.Value = val;
                        if (int.TryParse(jobIdString, out jobId))
                            bit.JobId = jobId;
                        else
                            bit.JobId = null;
                    }
                }
            }
            abRepo.SaveChanges();

            return RedirectToAction("Edit", new { id = paymentId });
        }

        public ActionResult NewBit(int id)
        {
            var payment = abRepo.ClientPayment(id);
            if (payment == null)
                return HttpNotFound();

            payment.Bits.Add(new ClientPaymentBit());
            abRepo.SaveChanges();

            return RedirectToAction("Edit", new { id = payment.Id });
        }
        public ActionResult DeleteBit(int bitId)
        {
            var bit = abRepo.ClientPaymentBit(bitId);
            if (bit == null)
                return HttpNotFound();

            int paymentId = bit.ClientPaymentId;
            abRepo.DeleteClientPaymentBit(bit);

            return RedirectToAction("Edit", new { id = paymentId });
        }

        public ActionResult EditViaLink(int id, DateTime? date, decimal value)
        {
            var clientPayment = abRepo.ClientPayment(id);
            if (clientPayment == null)
                return HttpNotFound();

            if (date.HasValue)
                clientPayment.Date = date.Value;
            if (clientPayment.Bits != null && clientPayment.Bits.Count() == 1)
            {
                clientPayment.Bits.First().Value = value;
            }
            abRepo.SaveChanges();
            return RedirectToAction("Show", "Clients", new { id = clientPayment.ClientId });
        }
    }
}