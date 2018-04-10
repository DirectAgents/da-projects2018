using DirectAgents.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DirectAgents.Domain.Entities.Cake
{
    public class Offer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OfferId { get; set; }
        public string OfferName { get; set; }

        public Nullable<int> AdvertiserId { get; set; }
        public virtual Advertiser Advertiser { get; set; }

        public Nullable<int> VerticalId { get; set; }
        public virtual Vertical Vertical { get; set; }

        public Nullable<int> OfferTypeId { get; set; }
        public virtual OfferType OfferType { get; set; }

        public Nullable<int> OfferStatusId { get; set; }
        public virtual OfferStatus OfferStatus { get; set; }

        public bool Hidden { get; set; }
        public string DefaultPriceFormatName { get; set; }
        public string CurrencyAbbr { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual ICollection<OfferContract> OfferContracts { get; set; }

        public virtual List<OfferBudget> OfferBudgets { get; set; }

        // Note: currently the Offer can only have one OfferBudget
        public OfferBudget OfferBudget
        {
            get // create one if none exist
            {
                if (OfferBudgets == null)
                    OfferBudgets = new List<OfferBudget>();

                if (OfferBudgets.Count == 0)
                    OfferBudgets.Add(new OfferBudget());

                return OfferBudgets[0];
            }
        }

        [NotMapped]
        public bool HasBudget
        {
            //get { return (OfferBudgets == null || OfferBudgets.Count > 0); }
            get { return (OfferBudgets != null && OfferBudgets.Count > 0); }
        }

        [NotMapped]
        public decimal? Budget
        {
            get { return HasBudget ? (decimal?)OfferBudget.Budget : null; }
            set { if (value.HasValue) OfferBudget.Budget = value.Value; }
        }
        [NotMapped]
        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime? BudgetStart
        {
            get { return HasBudget ? (DateTime?)OfferBudget.Start : null; }
            set { if (value.HasValue) OfferBudget.Start = value.Value; }
        }
        [NotMapped]
        [DataType(DataType.Date)]
        public DateTime? BudgetEnd
        {
            get { return HasBudget ? (DateTime?)OfferBudget.End : null; }
            set { if (value.HasValue) OfferBudget.End = value.Value; }
        }

        // --- misc ---

        [NotMapped]
        public decimal? BudgetUsed { get; set; }
        [NotMapped]
        public decimal? BudgetAvailable
        {
            get
            {
                if (!Budget.HasValue || !BudgetUsed.HasValue)
                    return null;
                if (BudgetUsed.Value >= Budget.Value)
                    return 0;
                else
                    return Budget.Value - BudgetUsed.Value;
            }
        }
        [NotMapped]
        public decimal? BudgetUsedPercent
        {
            get
            {
                if (!Budget.HasValue || Budget.Value <= 0 || !BudgetUsed.HasValue)
                    return null;
                return BudgetUsed.Value / Budget.Value;
            }
        }

        [NotMapped]
        public DateTime? EarliestStatDate { get; set; }
        [NotMapped]
        public DateTime? LatestStatDate { get; set; }
    }
}
