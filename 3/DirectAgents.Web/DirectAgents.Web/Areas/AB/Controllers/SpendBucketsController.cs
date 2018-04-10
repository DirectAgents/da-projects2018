using System;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Web.Areas.AB.Controllers
{
    public class SpendBucketsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public SpendBucketsController(IABRepository abRepository)
        {
            this.abRepo = abRepository;
        }

        // if campId==null, will also create the campaign
        public ActionResult New(int acctId, int? campId)
        {
            var clientAccount = abRepo.ClientAccount(acctId);
            if (clientAccount == null)
                return HttpNotFound();

            var spendBucket = new SpendBucket();
            if (campId.HasValue)
            {
                var campaign = abRepo.Campaign(campId.Value);
                if (campaign == null)
                    return HttpNotFound();
                spendBucket.CampaignId = campaign.Id;
            }
            else
            {
                spendBucket.Campaign = new Campaign
                {
                    Name = "zNew"
                };
            }
            clientAccount.SpendBuckets.Add(spendBucket);
            abRepo.SaveChanges();

            return RedirectToAction("BucketList", "Campaigns", new { acctId = clientAccount.Id });
        }

    }
}