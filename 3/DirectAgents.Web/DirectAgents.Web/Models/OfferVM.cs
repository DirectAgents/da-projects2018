using DirectAgents.Domain.Entities.Cake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectAgents.Web.Models
{
    public class OfferVM
    {
        // pass in the OfferDailySummaries needed to compute amount of budget used for the current period
        public OfferVM(Offer offer, IQueryable<OfferDailySummary> offerDailySummaries)
        {
            this.Offer = offer;
            this.OfferDailySummaries = offerDailySummaries;
        }

        public Offer Offer { get; set; }
        public IQueryable<OfferDailySummary> OfferDailySummaries { get; set; }

        private bool? _any;
        public bool Any
        {
            get
            {
                if (_any == null)
                    _any = OfferDailySummaries.Any();
                return _any.Value;
            }
        }

        public int Views
        {
            get { return Any ? OfferDailySummaries.Sum(o => o.Views) : 0; }
        }
        public int Clicks
        {
            get { return Any ? OfferDailySummaries.Sum(o => o.Clicks) : 0; }
        }
        public int Conversions
        {
            get { return Any ? OfferDailySummaries.Sum(o => o.Conversions) : 0; }
        }
        public int Paid
        {
            get { return Any ? OfferDailySummaries.Sum(o => o.Paid) : 0; }
        }
        public int Sellable
        {
            get { return Any ? OfferDailySummaries.Sum(o => o.Sellable) : 0; }
        }

        private decimal? _revenue;
        public decimal Revenue
        {
            get
            {
                if (_revenue == null)
                {
                    if (Any)
                        _revenue = OfferDailySummaries.Sum(o => o.Revenue);
                    else
                        _revenue = 0;
                }
                return _revenue.Value;
            }
        }
        public decimal Cost
        {
            get { return Any ? OfferDailySummaries.Sum(o => o.Cost) : 0; }
        }

        public DateTime? EarliestStatDate
        {
            get
            {
                if (Any)
                    return OfferDailySummaries.Min(o => o.Date);
                else
                    return null;
            }
        }

        public DateTime? LatestStatDate
        {
            get
            {
                if (Any)
                    return OfferDailySummaries.Max(o => o.Date);
                else
                    return null;
            }
        }

        public decimal? BudgetRemaining
        {
            get
            {
                decimal? remaining = null;
                if (Offer.Budget.HasValue)
                {
                    if (Revenue >= Offer.Budget.Value)
                        remaining = 0;
                    else
                        remaining = Offer.Budget.Value - Revenue;
                }
                return remaining;
            }
        }

        public decimal? BudgetUsedPercent
        {
            get
            {
                decimal? pctUsed = null;
                if (Offer.Budget.HasValue && Offer.Budget.Value != 0)
                    pctUsed = Revenue / Offer.Budget.Value;
                return pctUsed;
            }
        }
    }
}