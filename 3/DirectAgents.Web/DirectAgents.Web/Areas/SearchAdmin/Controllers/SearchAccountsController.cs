using System;
using System.Linq;
using System.Web.Mvc;
using CakeExtracter.Commands;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.CPSearch;

namespace DirectAgents.Web.Areas.SearchAdmin.Controllers
{
    public class SearchAccountsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public SearchAccountsController(ICPSearchRepository cpSearchRepository)
        {
            this.cpSearchRepo = cpSearchRepository;
        }

        public ActionResult Index(int? spId)
        {
            var searchAccounts = cpSearchRepo.SearchAccounts(spId: spId);
            return View(searchAccounts.OrderBy(x => x.SearchProfileId));
        }

        public ActionResult IndexGauge(int? spId, string channel)
        {
            var searchAccounts = cpSearchRepo.SearchAccounts(spId: spId, channel: channel, includeGauges: true);
            return View(searchAccounts.OrderBy(x => x.SearchProfileId));
        }

        public ActionResult Create(int spId, string channel)
        {
            var searchAccount = new SearchAccount
            {
                SearchProfileId = spId,
                Channel = channel,
                Name = "New"
            };
            cpSearchRepo.SaveSearchAccount(searchAccount, createIfDoesntExist: true);
            return RedirectToAction("Index", "SearchAccounts", new { spId = spId });
        }

        public ActionResult Edit(int id)
        {
            var searchAccount = cpSearchRepo.GetSearchAccount(id);
            if (searchAccount == null)
                return HttpNotFound();
            return View(searchAccount);
        }
        [HttpPost]
        public ActionResult Edit(SearchAccount searchAccount)
        {
            if (ModelState.IsValid)
            {
                if (cpSearchRepo.SaveSearchAccount(searchAccount))
                    return RedirectToAction("Index", new { spId = searchAccount.SearchProfileId });
                ModelState.AddModelError("", "SearchAccount could not be saved.");
            }
            return View(searchAccount);
        }

        [HttpPost]
        public ActionResult Sync(int id, DateTime? from, DateTime? to)
        {
            var searchAccount = cpSearchRepo.GetSearchAccount(id);
            if (searchAccount == null)
                return HttpNotFound();

            switch (searchAccount.Channel)
            {
                case SearchAccount.GoogleChannel:
                    SynchSearchDailySummariesAdWordsCommand.RunStatic(clientId: searchAccount.AccountCode, start: from, end: to, getAllStats: true);
                    break;
                case SearchAccount.BingChannel:
                    int accountId;
                    if (int.TryParse(searchAccount.AccountCode, out accountId))
                        SynchSearchDailySummariesBingCommand.RunStatic(accountId: accountId, start: from, end: to, getConversionTypeStats: true);
                    break;
                case SearchAccount.AppleChannel:
                    SynchSearchDailySummariesAppleCommand.RunStatic(clientId: searchAccount.AccountCode, start: from, end: to);
                    break;
            }

            if (searchAccount.SearchProfileId == null)
                return Content("SearchProfileId not set.");
            else
                return RedirectToAction("IndexGauge", new { spId = searchAccount.SearchProfileId.Value });
        }
    }
}