using System.ComponentModel.Composition;
using System.Net;

namespace CakeExtracter.Bootstrappers
{
    [Export(typeof(IBootstrapper))]
    public class ServicePointBootstrapper : IBootstrapper
    {
        public void Run()
        {
            ServicePointManager.DefaultConnectionLimit = 100;     
        }
    }
}
