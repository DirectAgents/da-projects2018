using System.Linq;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Domain.Seed
{
    public class ABSeeder
    {
        // Caller is responsible for instantiating and disposing of the context(s)
        public ABContext context { get; set; }

        public ABSeeder(ABContext context)
        {
            this.context = context;
        }

        public void SeedUnitTypes()
        {
            AddUnitTypeIfNotExist(4, "CPA", "CPA");
            AddUnitTypeIfNotExist(7, "CPM", "CPM");
            AddUnitTypeIfNotExist(8, "CPC", "CPC");
            AddUnitTypeIfNotExist(14, "APM", "Affiliate Program Mgmt");
            AddUnitTypeIfNotExist(15, "PPC", "PPC");
            AddUnitTypeIfNotExist(16, "CPI", "CPI");
            AddUnitTypeIfNotExist(18, "TD", "Trading Desk");
            AddUnitTypeIfNotExist(19, "SEO", "SEO");
            AddUnitTypeIfNotExist(20, "Crea", "Creative");
            AddUnitTypeIfNotExist(22, "Soc", "Social Media");
        }

        public void AddUnitTypeIfNotExist(int id, string abbrev, string name)
        {
            if (!context.UnitTypes.Any(x => x.Id == id))
            {
                var unitType = new UnitType
                {
                    Id = id,
                    Abbrev = abbrev,
                    Name = name
                };
                context.UnitTypes.Add(unitType);
                context.SaveChanges();
            }
        }
    }
}
