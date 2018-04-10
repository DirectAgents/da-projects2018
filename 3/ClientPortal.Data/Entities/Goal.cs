using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Contexts
{
    public partial class Goal
    {
        [NotMapped]
        public bool IsMonthly
        {
            get { return (this.StartDate == null && this.EndDate == null); }
        }
    }
}
