
namespace EomTool.Domain.DTOs
{
    public class CampAffId
    {
        public int pid { get; set; }
        public int affid { get; set; }
        public int? CostCurrId { get; set; }

        public CampAffId(int pid, int affid)
        {
            this.pid = pid;
            this.affid = affid;
        }
    }
}
