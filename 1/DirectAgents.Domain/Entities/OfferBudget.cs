using DirectAgents.Domain.Entities.Cake;
using System;

namespace DirectAgents.Domain.Entities
{
    public class OfferBudget
    {
        public int OfferBudgetId { get; set; }

        public int OfferId { get; set; }
        public virtual Offer Offer { get; set; }

        public decimal Budget { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
