using System;
using System.Collections.Generic;
using DirectAgents.Domain.Entities.RevTrack;

namespace DirectAgents.Domain.DTO
{
    public class ProgClientStats : TDLineItem
    {
        public ProgClient ProgClient { get; set; }
        public IEnumerable<ITDLineItem> LineItems { get; set; }
        public DateTime Month;

        public ProgClientStats(ProgClient progClient, IEnumerable<ITDLineItem> lineItems, DateTime monthStart, decimal? budget = null)
            : base(lineItems, budget)
        {
            ProgClient = progClient;
            LineItems = lineItems;
            Month = monthStart;
        }
    }
}
