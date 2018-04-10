using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace EomTool.Domain.Entities
{
    public partial class InvoiceStatus
    {
        public const int Default = 0;
        public const int AccountManagerReview = 1;
        public const int AccountingReview = 2;
        public const int Generated = 3;
    }

    public partial class Invoice
    {
        [NotMapped]
        public Advertiser Advertiser { get; set; }

        //[NotMapped]
        public List<InvoiceLineItem> LineItems = new List<InvoiceLineItem>();

        [NotMapped]
        public string CurrencyName
        {
            get
            {
                var currencyNames = InvoiceItems.Select(i => i.CurrencyName).Distinct();
                return ComputeCurrencyName(currencyNames);
            }
        }
        private string ComputeCurrencyName(IEnumerable<string> currencyNames)
        {
            if (currencyNames.Count() > 1)
                return "mixed";
            else if (currencyNames.Count() == 1)
                return currencyNames.First();
            else
                return null;
        }

        public string CampaignCurrencyName(int pid)
        {
            var campaignLineItems = LineItems.Where(li => li.Campaign.pid == pid);
            var currencyNames = campaignLineItems.Select(li => (li.Currency != null ? li.Currency.name : null)).Distinct();
            return ComputeCurrencyName(currencyNames);
        }

        public decimal CampaignSubTotal(int pid)
        {
            var campaignLineItems = LineItems.Where(li => li.Campaign.pid == pid);
            return campaignLineItems.Sum(li => li.TotalAmount);
        }

        [NotMapped]
        public decimal Total
        {
            get { return InvoiceItems.Sum(i => i.total_amount.HasValue ? i.total_amount.Value : 0); }
        }

        [NotMapped]
        public DateTime DateRequested
        {
            get { return (FirstNote != null) ? FirstNote.created : new DateTime(2000, 1, 1); }
        }

        [NotMapped]
        public InvoiceNote FirstNote
        {
            get
            {
                if (InvoiceNotes.Count == 0)
                    return null;
                else
                    return InvoiceNotes.OrderBy(n => n.created).First();
            }
        }

        [NotMapped]
        public InvoiceNote LatestNote
        {
            get
            {
                if (InvoiceNotes.Count == 0)
                    return null;
                else
                    return InvoiceNotes.OrderBy(n => n.created).Last();
            }
        }
        [NotMapped]
        public string LatestNoteString
        {
            get
            {
                var latestNote = LatestNote;
                return (latestNote == null) ? null : latestNote.note;
            }
        }

        public void AddNote(string from, string note)
        {
            var invoiceNote = new InvoiceNote
            {
                added_by = from,
                note = note,
                created = DateTime.Now
            };
            this.InvoiceNotes.Add(invoiceNote);
        }
    }

    // intended for one campaign (pid)
    public class InvoiceLineItem
    {
        public Campaign Campaign { get; set; }
        public Currency Currency { get; set; }

        public string ItemCode { get; set; }

        public IEnumerable<InvoiceItem> SubItems { get; set; }

        public decimal TotalAmount
        {
            get { return SubItems.Sum(i => i.total_amount.HasValue ? i.total_amount.Value : 0); }
        }
        public int NumUnits
        {
            get { return SubItems.Sum(i => i.num_units); }
        }
        public decimal AmountPerUnit
        {
            get { return TotalAmount / NumUnits; }
        }
    }

    public partial class InvoiceItem
    {
        [NotMapped]
        public string AffiliateName { get; set; }

        public string _currencyName = null;
        [NotMapped]
        public string CurrencyName
        {
            set { _currencyName = value; }
            get
            {
                if (_currencyName == null && Currency != null)
                    _currencyName = Currency.name;
                return _currencyName;
            }
        }

        public string _unitTypeName = null;
        [NotMapped]
        public string UnitTypeName
        {
            set { _unitTypeName = value; }
            get
            {
                if (_unitTypeName == null && UnitType != null)
                    _unitTypeName = UnitType.name;
                return _unitTypeName;
            }
        }

        [NotMapped]
        public decimal TotalAmount0
        {
            get { return (total_amount.HasValue ? total_amount.Value : 0); }
        }
    }

    public partial class InvoiceNote
    {
        [NotMapped]
        public string AddedBy_IdOnly
        {
            get
            {
                if (added_by != null && added_by.IndexOf("DIRECTAGENTS\\") == 0)
                    return added_by.Substring(13);
                else
                    return added_by;
            }
        }
    }
}
