using System.Linq;
using EomTool.Domain.Abstract;
using EomTool.Domain.Entities;

namespace EomTool.Domain.Concrete
{
    public class AffiliateRepository : IAffiliateRepository
    {
        EomEntities context;

        public AffiliateRepository(EomEntities context)
        {
            this.context = context;
        }

        public IQueryable<Affiliate> Affiliates
        {
            get
            {
                return context.Affiliates;
            }
        }

        public Affiliate AffiliateByAffId(int affid)
        {
            return context.Affiliates.Where(a => a.affid == affid).FirstOrDefault();
        }
    }
}
