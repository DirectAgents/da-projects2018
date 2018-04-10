using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using EomTool.Domain.Abstract;

namespace EomTool.Domain.Entities
{
    public partial class Item
    {
        [NotMapped]
        public string AdvertiserName { get; set; }
        [NotMapped]
        public string CampaignName { get; set; }
        [NotMapped]
        public string AffiliateName { get; set; }
        [NotMapped]
        public string RevenueCurrencyName { get; set; }
        [NotMapped]
        public string CostCurrencyName { get; set; }
        [NotMapped]
        public string UnitTypeName { get; set; }
        [NotMapped]
        public string SourceName { get; set; }

        [NotMapped]
        public string AccountManagerName { get; set; }
        [NotMapped]
        public string AdManagerName { get; set; }
        [NotMapped]
        public string MediaBuyerName { get; set; }

        public string RevenuePerUnitString()
        {
            return this.revenue_per_unit.ToString("0.00##");
        }
        public string CostPerUnitString()
        {
            return this.cost_per_unit.ToString("0.00##");
        }
        public string NumUnitsString()
        {
            return this.num_units.ToString("0.######");
        }
        public string TotalRevenueString()
        {
            return this.total_revenue.HasValue ? this.total_revenue.Value.ToString("0.00########") : "";
        }
        public string TotalCostString()
        {
            return this.total_cost.HasValue ? this.total_cost.Value.ToString("0.00########") : "";
        }
        public string MarginString()
        {
            return this.margin.HasValue ? this.margin.Value.ToString("0.00##") : "";
        }

        [NotMapped]
        public static List<string> SettableProperties
        {
            get { return (new string[] { "pid", "affid", "revenue", "cost", "notes", "type", "units" }).ToList(); }
        }

        public void SetDefaults()
        {
            this.pid = -1;
            this.affid = -1;

            this.UnitTypeName = "CPM";
            this.num_units = 1;
            this.RevenueCurrencyName = "USD";
            this.CostCurrencyName = "USD";

            this.SourceName = "Other";
            this.accounting_notes = "none";
            SetDefaultStatuses();
            //SetDefaultTypes();
        }
        public void SetDefaultStatuses()
        {
            this.item_accounting_status_id = 1;
            this.item_reporting_status_id = 1;
            this.campaign_status_id = 1;
            this.media_buyer_approval_status_id = 1;
        }

        public void SetDefaultTypes()
        {
            this.cost_currency_id = 1;
            this.revenue_currency_id = 1;
            this.source_id = Source.Other;
            this.unit_type_id = UnitType.CPA;
        }
        // source_id, unit_type_id, revenue_currency_id, cost_currency_id

        // returns null if success
        public string SetProperty(string prop, string value, IMainRepository mainRepo = null)
        {
            prop = prop.ToLower().Trim();
            if (value != null)
                value = value.Trim();
            if (String.IsNullOrEmpty(value) && prop != "notes")
                return null;

            string error = null;
            switch (prop)
            {
                case "pid":
                    int pid;
                    if (!Int32.TryParse(value, out pid))
                        error = "PID must be a number";
                    else
                        this.pid = pid;
                    break;
                case "affid":
                    int affid;
                    if (!Int32.TryParse(value, out affid))
                        error = "AffId must be a number";
                    else
                        this.affid = affid;
                    break;
                case "revenue":
                    decimal amount;
                    string currency;
                    if (!Util.ParseMoney(value, out amount, out currency))
                        error = "Could not parse revenue";
                    else
                    {
                        this.revenue_per_unit = amount;
                        this.RevenueCurrencyName = currency;
                    }
                    break;
                case "cost":
                    if (!Util.ParseMoney(value, out amount, out currency))
                        error = "Could not parse cost";
                    else
                    {
                        this.cost_per_unit = amount;
                        this.CostCurrencyName = currency;
                    }
                    break;
                case "notes":
                    this.notes = value;
                    break;
                case "type":
                    this.UnitTypeName = value;
                    break;
                case "units":
                    int units;
                    if (!int.TryParse(value, out units))
                        error = "Could not parse units";
                    else
                        this.num_units = units;
                    break;
                default:
                    error = String.Format("Property '{0}' cannot be set", prop);
                    break;
            }
            return error;
        }

        // also sets extended properties (e.g. campaign name)
        public List<string> VerifyAndFinalizeProperties(IMainRepository mainRepo)
        {
            List<string> errors = new List<string>();

            if (pid < 0)
                errors.Add("Campaign not found");
            else
            {
                var campaign = mainRepo.GetCampaign(pid);
                if (campaign == null)
                    errors.Add(String.Format("Campaign with PID {0} does not exist", pid));
                else
                {
                    CampaignName = campaign.campaign_name;
                    AdvertiserName = campaign.Advertiser.name;
                }
            }
            if (affid < 0)
                errors.Add("Affiliate not found");
            else
            {
                var affiliate = mainRepo.GetAffiliate(affid);
                if (affiliate == null)
                    errors.Add(String.Format("Affiliate with AffId {0} does not exist", affid));
                else
                    AffiliateName = affiliate.name;
            }

            if (notes == null)
                errors.Add("Notes are required");

            var source = mainRepo.GetSource(SourceName);
            if (source == null)
                errors.Add(String.Format("Source '{0}' does not exist", SourceName));
            else
                source_id = source.id;

            var unitType = mainRepo.GetUnitType(UnitTypeName);
            if (unitType == null)
                errors.Add(String.Format("Unit Type '{0}' does not exist", UnitTypeName));
            else
                unit_type_id = unitType.id;

            if (num_units < 0)
                errors.Add("Units cannot be negative");

            var revCurrency = mainRepo.GetCurrency(RevenueCurrencyName);
            if (revCurrency == null)
                errors.Add(String.Format("Revenue Currency '{0}' does not exist", RevenueCurrencyName));
            else
                revenue_currency_id = revCurrency.id;

            var costCurrency = mainRepo.GetCurrency(CostCurrencyName);
            if (costCurrency == null)
                errors.Add(String.Format("Cost Currency '{0}' does not exist", CostCurrencyName));
            else
                cost_currency_id = costCurrency.id;

            return errors;
        }
    }
}
