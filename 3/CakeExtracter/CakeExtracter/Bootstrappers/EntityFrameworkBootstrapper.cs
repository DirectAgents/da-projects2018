using System.ComponentModel.Composition;
using System.Data.Entity;
using CakeExtracter.Data;

namespace CakeExtracter.Bootstrappers
{
    [Export(typeof(IBootstrapper))]
    public class EntityFrameworkBootstrapper : IBootstrapper
    {
        public void Run()
        {
            Database.SetInitializer<UsersContext>(null);
        }
    }
}
