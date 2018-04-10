using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.Screen
{
    public class SalespersonStat
    {
        public virtual Salesperson Salesperson { get; set; }

        public int SalespersonId { get; set; }
        public DateTime Date { get; set; }

        public int EmailSent { get; set; }
        public int EmailTracked { get; set; }
        public int EmailOpened { get; set; }
        public int EmailReplied { get; set; }

        [NotMapped]
        public double EmailOpenRate
        {
            get { return (EmailTracked == 0) ? 0 : Math.Round((double)EmailOpened / EmailTracked, 4); }
        }
        [NotMapped]
        public double EmailReplyRate
        {
            get { return (EmailTracked == 0) ? 0 : Math.Round((double)EmailReplied / EmailTracked, 4); }
        }

        public SalespersonStat()
        {
        }
        public SalespersonStat(SalespersonStat stat)
        {
            // Don't copy Salesperson object
            SalespersonId = stat.SalespersonId;
            Date = stat.Date;
            EmailSent = stat.EmailSent;
            EmailTracked = stat.EmailTracked;
            EmailOpened = stat.EmailOpened;
            EmailReplied = stat.EmailReplied;
        }

        public static SalespersonStat BlankStat(int? salespersonId, DateTime? date)
        {
            var stat = new SalespersonStat();
            if (salespersonId.HasValue)
                stat.SalespersonId = salespersonId.Value;
            if (date.HasValue)
                stat.Date = date.Value;
            return stat;
        }
    }
}
