using System;
using System.Collections.Generic;

namespace DirectAgents.Domain.Entities.AB
{
    public class ProtoInvoice
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public virtual ABClient Client { get; set; }
        public DateTime Date { get; set; }
        // Invoice #, etc
        // Terms, (calculated?)DueDate

        public virtual ICollection<ProtoInvoiceBit> Bits { get; set; }
    }

    public class ProtoInvoiceBit
    {
        public int Id { get; set; }
        public int ProtoInvoiceId { get; set; }
        public virtual ProtoInvoice ProtoInvoice { get; set; }

        public decimal Value { get; set; }

        // Allocation? - AcctingPeriod, campaign, etc (nullable)

        public virtual ICollection<ProtoPaymentBit> ProtoPaymentBits { get; set; } // the payments received to cover this invoice item

        //public decimal AmountUnpaid() { } // Value minus sum of paymentbit values

        public int? ProtoSpendBitId { get; set; }
        public virtual ProtoSpendBit ProtoSpendBit { get; set; }
        public int? ClientAccountId { get; set; }
        public virtual ClientAccount ClientAccount { get; set; }
        // only one of the two should be non-null ?
    }
}
