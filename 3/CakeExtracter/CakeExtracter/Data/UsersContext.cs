using System.Data.Entity;
using ClientPortal.Web.Models.Cake;

namespace CakeExtracter.Data
{
    // HACK: this is in two places right now because referencing the web project causes a runtime error..
    internal class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }
        public DbSet<conversion> Conversions { get; set; }
        public DbSet<click> Clicks { get; set; }
    }
}