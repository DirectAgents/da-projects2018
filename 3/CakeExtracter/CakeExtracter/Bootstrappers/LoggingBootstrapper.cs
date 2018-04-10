using System.ComponentModel.Composition;
using CakeExtracter.Logging.Loggers;

namespace CakeExtracter.Bootstrappers
{
    [Export(typeof(IBootstrapper))]
    public class LoggingBootstrapper : IBootstrapper
    {
        public void Run()
        {
            Logger.Instance = new EnterpriseLibraryLogger();
            //Logger.Instance = new ConsoleLogger();                   
        }
    }
}
