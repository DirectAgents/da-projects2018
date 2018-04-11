using System.ComponentModel.Composition;
using CakeExtracter.Common;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Seed;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASeed : ConsoleCommand
    {
        public string Type { get; set; }

        public override void ResetProperties()
        {
            this.Type = null;
        }

        public DASeed()
        {
            IsCommand("daSeed", "Seed the database");
            HasOption("t|type=", "What type of seeding to do", c => this.Type = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            //if (this.Type.ToUpper() == "TD")
                DoTDSeed();
            return 0;
        }

        public void DoTDSeed()
        {
            using (var db = new ClientPortalProgContext())
            {
                var tdSeeder = new TDSeeder(db);
                tdSeeder.SeedNetworks();
                tdSeeder.SeedPlatforms();
            }
        }
    }
}
