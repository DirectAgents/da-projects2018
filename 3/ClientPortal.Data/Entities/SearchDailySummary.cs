using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Contexts
{
    public partial class SearchDailySummary
    {
        [NotMapped]
        public int Calls { get; set; }
    }
}
