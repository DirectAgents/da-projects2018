using System;
using System.Collections.Generic;
using DirectAgents.Domain.DTO;

namespace DirectAgents.Domain.Abstract
{
    public interface IDepartmentRepository
    {
        IEnumerable<IRTLineItem> StatsByClient(DateTime monthStart, bool includeZeros = false, int? maxClients = null);
        IRTLineItem StatSummaryForClient(int abClientId, DateTime monthStart);
        //IEnumerable<IRTLineItem> StatBreakdownByVendor ?
        IEnumerable<IRTLineItem> StatBreakdownByLineItem(int abClientId, DateTime monthStart, bool separateFees = false, bool combineFees = false);
    }
}
