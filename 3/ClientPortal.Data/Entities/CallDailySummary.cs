using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Contexts
{
    public partial class CallDailySummary
    {
        [NotMapped]
        public string LCcmpid { get; set; }
    }
}
