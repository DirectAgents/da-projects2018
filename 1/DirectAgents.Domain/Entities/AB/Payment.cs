using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DirectAgents.Domain.Entities.AB
{
    public class Payment
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public virtual ABClient Client { get; set; }
        public DateTime Date { get; set; }

        public virtual ICollection<PaymentBit> Bits { get; set; }

        public decimal TotalValue()
        {
            if (Bits != null && Bits.Any())
                return Bits.Sum(b => b.Value);
            else
                return 0;
        }
    }

    public class PaymentBit
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public virtual Payment Payment { get; set; }

        public int AcctId { get; set; }
        [ForeignKey("AcctId")]
        public virtual ClientAccount ClientAccount { get; set; }

        public decimal Value { get; set; }
    }
}
