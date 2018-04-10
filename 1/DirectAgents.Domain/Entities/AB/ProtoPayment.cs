using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectAgents.Domain.Entities.AB
{
    public class ProtoPayment
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public virtual ABClient Client { get; set; }
        public DateTime Date { get; set; }

        public virtual ICollection<ProtoPaymentBit> Bits { get; set; }

        public decimal TotalValue()
        {
            if (Bits != null && Bits.Any())
                return Bits.Sum(b => b.Value);
            else
                return 0;
        }
    }

    public class ProtoPaymentBit
    {
        public int Id { get; set; }
        public int ProtoPaymentId { get; set; }
        public virtual ProtoPayment ProtoPayment { get; set; }
        public decimal Value { get; set; }

        public int? ProtoInvoiceBitId { get; set; }
        public virtual ProtoInvoiceBit ProtoInvoiceBit { get; set; }
        public int? ClientAccountId { get; set; }
        public virtual ClientAccount ClientAccount { get; set; }
        // only one of the two should be non-null ?

        //public int? JobId { get; set; }
    }
}
