using System;
using System.Collections.Generic;
using DirectAgents.Domain.DTO;

namespace DirectAgents.Domain.Abstract
{
    public interface ISuperRepository
    {
        void SetRepositories(IMainRepository mainRepo, IRevTrackRepository rtRepo, IABRepository abRepo);

        IEnumerable<ABStat> StatsByClient(DateTime monthStart, int? maxClients = null);
        IEnumerable<ABStat> StatsForClient(int abClientId, DateTime monthStart); // by department
        IEnumerable<ABStat> StatsByVendor(int abClientId, DateTime monthStart);
        IEnumerable<ABLineItem> StatsByLineItem(int abClientId, DateTime monthStart, bool separateFees = false, bool combineFees = false);
    }
}
