using System;

namespace ClientPortal.Data.DTOs
{
    public class MonthlyInfo
    {
        public string Id { get { return OfferId + "_" + Period.ToString("yyyyMM"); } }

        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime Period
        {
            get { return new DateTime(this.Year, this.Month, 1); }
        }
        public int AdvertiserId { get; set; }
        //public string AdvertiserName { get; set; }
        public int OfferId { get; set; }
        public string Offer { get; set; }
        //public string UnitType { get; set; }
        //public int TotalUnits { get; set; }

        public int CampaignStatusId { get; set; }
        public int AccountingStatusId { get; set; }
        public string AccountingStatusName
        {
            get { return AccountingStatus.IdToName(this.AccountingStatusId); }
        }

        public decimal Revenue { get; set; }
        public string Currency
        {
            set { Culture = OfferInfo.CurrencyToCulture(value); }
        }
        public string Culture { get; set; }
    }

    public partial class CampaignStatus
    {
        public const int Default = 1;
        public const int Finalized = 2;
        public const int Active = 3;
        public const int Verified = 4;
    }
    public partial class AccountingStatus
    {
        public const int Default = 1;
        public const int PaymentDue = 2;
        public const int DoNotPay = 3;
        public const int CheckCut = 4;
        public const int CheckSignedAndPaid = 5;
        public const int Approved = 6;
        public const int Hold = 7;
        public const int Verified = 8;

        public static string IdToName(int accountingStatusId)
        {
            switch (accountingStatusId)
            {
                case Default:
                    return "Unverified";
                case CheckSignedAndPaid:
                    return "Paid";
                case Approved:
                    return "Approved";
                case Verified:
                    return "Verified";
                case Hold:
                    return "Held";
                default:
                    return String.Empty;
            }
        }
    }
}
