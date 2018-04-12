using System.Linq;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Domain.Seed
{
    public class TDSeeder
    {
        // Caller is responsible for instantiating and disposing of the context(s)
        public ClientPortalProgContext context { get; set; }

        public TDSeeder(ClientPortalProgContext context)
        {
            this.context = context;
        }

        public void SeedPlatforms()
        {
            AddPlatformIfNotExist(Platform.Code_AdRoll, "AdRoll");
            AddPlatformIfNotExist(Platform.Code_DBM, "DBM");
            AddPlatformIfNotExist(Platform.Code_FB, "Facebook");
            AddPlatformIfNotExist(Platform.Code_Twitter, "Twitter");
            AddPlatformIfNotExist(Platform.Code_Instagram, "Instagram");
            AddPlatformIfNotExist(Platform.Code_DATradingDesk, "DA Trading Desk");
            AddPlatformIfNotExist("mf", "Management Fee");
            AddPlatformIfNotExist("adel", "Adelphic");
            AddPlatformIfNotExist(Platform.Code_Adform, "Adform");
            AddPlatformIfNotExist("adt", "Adroit");
            AddPlatformIfNotExist(Platform.Code_Amazon, "Amazon");
            AddPlatformIfNotExist("apf", "AppsFlyer");
            AddPlatformIfNotExist("bc", "Bluecore");
            AddPlatformIfNotExist(Platform.Code_Criteo, "Criteo");
            AddPlatformIfNotExist("kin", "Kinetic");
            AddPlatformIfNotExist("li", "LiveIntent");
            AddPlatformIfNotExist("mg", "Magnetic");
            AddPlatformIfNotExist("max", "Maxpoint");
            AddPlatformIfNotExist("qnt", "Quantcast");
            AddPlatformIfNotExist("tab", "Taboola");
            AddPlatformIfNotExist("tc", "TapCommerce");
            AddPlatformIfNotExist(Platform.Code_YAM, "YAM");
            AddPlatformIfNotExist("zem", "Zemanta");
        }

        public void SeedNetworks()
        {
            AddNetworkIfNotExist(1, "Facebook");
            AddNetworkIfNotExist(2, "Instagram");
            AddNetworkIfNotExist(3, "Audience Network");
            AddNetworkIfNotExist(4, "Messenger");
        }

        public void AddPlatformIfNotExist(string code, string name)
        {
            if (!context.Platforms.Any(p => p.Code == code))
            {
                var platform = new Platform
                {
                    Code = code,
                    Name = name
                };
                context.Platforms.Add(platform);
                context.SaveChanges();
            }
        }
        public void AddNetworkIfNotExist(int id, string name)
        {
            if (!context.Networks.Any(x => x.Id == id))
            {
                var network = new Network
                {
                    Id = id,
                    Name = name
                };
                context.Networks.Add(network);
                context.SaveChanges();
            }
        }

    }
}
