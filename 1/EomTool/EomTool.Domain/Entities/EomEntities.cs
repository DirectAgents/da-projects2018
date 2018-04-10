using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Reflection;
using EomTool.Domain.Abstract;
using Ninject;

namespace EomTool.Domain.Entities
{
    public partial class EomEntities
    {
        [Inject]
        public EomEntities(IEomEntitiesConfig config)
            : base(CreateEntityConnection(config.ConnectionString), true)
        {
        }

        public EomEntities(string cs, bool dummy) // dummy ignored, used for overloading
            : base(CreateEntityConnection(cs), true)
        {
        }

        private static EntityConnection CreateEntityConnection(string databaseConnectionString)
        {
            return new EntityConnection(new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = databaseConnectionString + ";multipleactiveresultsets=True;App=EntityFramework",
                Metadata = @"res://*/Entities.EomModel.csdl|res://*/Entities.EomModel.ssdl|res://*/Entities.EomModel.msl"
            }.ConnectionString);
        }
    }

    public partial class CampaignStatus
    {
        public const int Default = 1;
        public const int Finalized = 2;
        public const int Active = 3;
        public const int Verified = 4;

        public static string DisplayVal(int? campaignStatus)
        {
            if (campaignStatus.HasValue)
                return DisplayValNotNullable(campaignStatus.Value);
            else
                return "All";
        }
        private static string DisplayValNotNullable(int campaignStatus)
        {
            var props = typeof(CampaignStatus).GetFields(BindingFlags.Public | BindingFlags.Static);
            var wantedProp = props.FirstOrDefault(prop => (int)prop.GetValue(null) == campaignStatus);
            return (wantedProp == null ? null : wantedProp.Name);
        }
    }

    public partial class ItemAccountingStatus
    {
        public const int Default = 1;
        public const int PaymentDue = 2;
        public const int DoNotPay = 3;
        public const int CheckCut = 4;
        public const int CheckSignedAndPaid = 5;
        public const int Approved = 6;
        public const int Hold = 7;
        public const int Verified = 8;
    }

    public partial class MediaBuyerApprovalStatus
    {
        public const int Default = 1;
        public const int Queued = 2;
        public const int Sent = 3;
        public const int Approved = 4;
        public const int Held = 5;
    }

    public partial class PaymentBatchState
    {
        public const int Default = 1;
        public const int Sent = 3;
        public const int Complete = 6;
    }

    public partial class Advertiser
    {
        [NotMapped]
        public AccountManager AccountManager { get; set; }

        [NotMapped]
        public Advertiser PreviousMonthAdvertiser { get; set; }
    }

    public partial class Affiliate
    {
        [NotMapped]
        public Affiliate PreviousMonthAffiliate { get; set; }
        [NotMapped]
        public bool AnalystRolesAssigned { get; set; } //legacy
    }

    public partial class AnalystManager
    {
        public String AnalystsNames(string separator = ", ")
        {
            if (Analysts == null)
                return null;
            return String.Join(separator, Analysts.Select(x => x.name));
        }
    }

    public partial class BatchUpdate
    {
        [NotMapped]
        public string Action
        {
            get
            {
                if (this.media_buyer_approval_status_id == MediaBuyerApprovalStatus.Approved
                    && this.from_media_buyer_approval_status_id == MediaBuyerApprovalStatus.Held)
                    return "Released";
                else
                    return (this.MediaBuyerApprovalStatus == null) ? null : this.MediaBuyerApprovalStatus.name;
            }
        }
    }

    public partial class Campaign
    {
        [NotMapped]
        public string DisplayName
        {
            get { return (String.IsNullOrWhiteSpace(display_name) ? campaign_name : display_name); }
        }
        [NotMapped]
        public IEnumerable<AnalystRole> AnalystRoles { get; set; }
        [NotMapped]
        public string AnalystNames
        {
            get //Note: if doing this more than once, should cache this
            {
                if (AnalystRoles != null)
                {
                    var peopleNames = AnalystRoles.Select(x => x.Person).ToList().Select(x => x.FullName);
                    return string.Join(", ", peopleNames);
                }
                else
                    return null;
            }
        }
        [NotMapped]
        public CampAff CampAff { get; set; }
    }

    public partial class CampAff
    {
        [NotMapped]
        public Campaign Campaign { get; set; }
        [NotMapped]
        public Affiliate Affiliate { get; set; }
    }

    public partial class MarginApproval
    {
        [NotMapped]
        public Campaign Campaign { get; set; }
        [NotMapped]
        public Affiliate Affiliate { get; set; }
    }

    public partial class PaymentBatch
    {
        [NotMapped]
        public string AccountingPeriod { get; set; }

        [NotMapped]
        public IEnumerable<PublisherPayment> Payments { get; set; }

        [NotMapped]
        public string approver_abbrev
        {
            get
            {
                if (approver_identity != null && approver_identity.StartsWith("DIRECTAGENTS\\"))
                    return (approver_identity.Substring(13));
                else return approver_identity;
            }
        }
    }

    public partial class PublisherPayment
    {
        [NotMapped]
        public string AccountingPeriod { get; set; }
        [NotMapped]
        public int NumNotes { get; set; }
        [NotMapped]
        public int NumAttachments { get; set; }
    }

    public partial class Source
    {
        public const int Cake = 9;
        public const int Other = 8;

        public static int? ToSourceId(string sourceName)
        {
            if (sourceName == null)
                return null;
            string sourceNameLowered = sourceName.Trim().ToLower();

            switch (sourceNameLowered)
            {
                case "cake":
                    return Source.Cake;
                case "other":
                    return Source.Other;
                default:
                    return null;
            }
        }
    }

    public partial class UnitType
    {
        public const int CPA = 4;
        public const int CPC = 8;
        public const int PPC = 15;
        public const int TradingDesk = 18;
        //etc...

        public static string ToItemCode(string unitTypeName)
        {
            if (unitTypeName == null)
                return null;
            string unitTypeNameLowered = unitTypeName.ToLower();

            if (unitTypeNameLowered.Contains("affiliate program"))
                return "Affiliate Management";
            switch (unitTypeNameLowered)
            {
                case "call tracking":
                    return "Search";
                case "gsp":
                    return "GSP";
                case "opm":
                    return "Affiiate Program Management";
                case "ppc":
                    return "Search Marketing";
                case "revshare":
                    return "CPA Fee";
                case "seo":
                    return "Search";
                case "trading desk":
                    return "Trading Desk";
                default:
                    if (unitTypeNameLowered.Contains(" fee"))
                        return unitTypeName;
                    else
                        return unitTypeName + " Fee";
            }
        }
    }

    public partial class Person
    {
        [NotMapped]
        public string FullName
        {
            get { return first_name + " " + last_name; }
        }
    }
}
