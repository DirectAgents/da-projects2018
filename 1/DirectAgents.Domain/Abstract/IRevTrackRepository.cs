using System;
using System.Linq;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities.RevTrack;

namespace DirectAgents.Domain.Abstract
{
    public interface IRevTrackRepository : IDisposable
    {
        void SaveChanges();

        ProgClient ProgClient(int id);
        IQueryable<ProgClient> ProgClients(int? ABClientId = null);
        ProgCampaign ProgCampaign(int id);
        IQueryable<ProgCampaign> ProgCampaigns();
        ProgVendor ProgVendor(int id);
        IQueryable<ProgVendor> ProgVendors();

        // Stats
        IQueryable<ProgSummary> ProgSummaries(DateTime? startDate, DateTime? endDate, int? clientId = null, int? campaignId = null, int? vendorId = null);
        ProgExtraItem ProgExtraItem(int id);
        IQueryable<ProgExtraItem> ProgExtraItems(DateTime? startDate, DateTime? endDate, int? clientId = null, int? campaignId = null, int? vendorId = null);

        ProgClientStats GetProgClientStats(DateTime monthStart, int clientId);
        ProgClientStats GetProgClientStats(DateTime monthStart, ProgClient progClient);
    }
}
