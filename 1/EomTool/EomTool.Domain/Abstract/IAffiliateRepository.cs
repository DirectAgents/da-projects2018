using System.Linq;
using EomTool.Domain.Entities;

namespace EomTool.Domain.Abstract
{
    public interface IAffiliateRepository
    {
        IQueryable<Affiliate> Affiliates { get; }
        Affiliate AffiliateByAffId(int affid);
    }
}
