using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CakeExtracter.CakeMarketingApi.Entities;
using EomTool.Domain.Abstract;
using EomTool.Domain.Entities;

namespace CakeExtracter.Etl.CakeMarketing.Loaders
{
    class CPCLoader : Loader<CampaignSummary>
    {
        private readonly DateTime date;
        private readonly IMainRepository mainRepo;

        public CPCLoader(DateTime date,IMainRepository mainRepository)
        {
            this.date = date;
            this.mainRepo = mainRepository;
        }

        //TODO: move code to Upsert...()
        // Check if campaign and/or affiliate exist in db; add if needed?

        protected override int Load(List<CampaignSummary> items)
        {
            foreach (var campaignItem in items)
            {
                if (campaignItem.SourceAffiliate.SourceAffiliateId > 0 && campaignItem.Sellable > 0)
                {
                    Item item = new Item
                    {
                        affid = campaignItem.SourceAffiliate.SourceAffiliateId,
                        pid = campaignItem.SiteOffer.SiteOfferId,
                        num_units = campaignItem.Sellable,
                        revenue_per_unit = campaignItem.Revenue / campaignItem.Sellable,
                        cost_per_unit = campaignItem.Cost / campaignItem.Sellable,
                        notes = "From Cake",
                        accounting_notes = "From Cake",
                        name = "Cake/" + date.ToString("yyyy-MM-dd") + "/aff:" + campaignItem.SourceAffiliate.SourceAffiliateId + "/offer:" + campaignItem.SiteOffer.SiteOfferId + "/type:Click - CPC"
                    };
                    item.SetDefaultStatuses();
                    item.SetDefaultTypes();
                    item.source_id = Source.Cake;
                    item.unit_type_id = UnitType.CPC;
                    mainRepo.AddItem(item);
                }
            }
            mainRepo.SaveChanges();
            return items.Count;
        }
    }
}
