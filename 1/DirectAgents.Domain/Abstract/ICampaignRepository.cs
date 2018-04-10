using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities.Wiki;

namespace DirectAgents.Domain.Abstract
{
    public interface ICampaignRepository
    {
        void SaveChanges();
        IQueryable<Campaign> Campaigns { get; }
        IQueryable<Campaign> CampaignsFiltered(string[] excludeInName, string search, string countrycode, string vertical, string traffictype, bool? mobilelp, bool excludeHidden, bool excludeInactive);

        IQueryable<Country> Countries { get; }
        IQueryable<Country> CountriesWithActiveCampaigns { get; }
        IQueryable<string> AllCountryCodes { get; }

        IQueryable<Vertical> Verticals { get; }
        IQueryable<TrafficType> TrafficTypes { get; }

        Campaign FindById(int pid);
        void SaveCampaign(Campaign campaign);
        //IEnumerable<CampaignSummary> TopCampaigns(int num, TopCampaignsBy by, string trafficType);
    }
}
