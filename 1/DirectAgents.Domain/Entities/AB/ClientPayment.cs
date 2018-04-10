using System;
using System.Collections.Generic;
using System.Linq;

namespace DirectAgents.Domain.Entities.AB
{
    public class ClientPayment
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public virtual ABClient Client { get; set; }
        public DateTime Date { get; set; }

        public virtual ICollection<ClientPaymentBit> Bits { get; set; }

        public decimal TotalValue()
        {
            if (Bits != null && Bits.Any())
                return Bits.Sum(b => b.Value);
            else
                return 0;
        }
        public decimal NonJobValue()
        {
            if (Bits == null || !Bits.Any())
                return 0;
            var bits = Bits.Where(b => b.JobId == null);
            return bits.Sum(b => b.Value);
        }
        public decimal JobValue()
        {
            if (Bits == null || !Bits.Any())
                return 0;
            var bits = Bits.Where(b => b.JobId.HasValue);
            return bits.Sum(b => b.Value);
        }
    }

    public class ClientPaymentBit
    {
        public int Id { get; set; }
        public int ClientPaymentId { get; set; }
        public virtual ClientPayment ClientPayment { get; set; }

        public decimal Value { get; set; }

        public int? JobId { get; set; }
        public virtual Job Job { get; set; }
    }
}
