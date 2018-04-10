using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Entities.TD.AdRoll
{
    public class AdRollProfile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? TradingDeskAccountId { get; set; }
        public virtual TradingDeskAccount TradingDeskAccount { get; set; }

        public string Name { get; set; }
        public string Eid { get; set; } // (Advertiseable Eid)
    }
}
