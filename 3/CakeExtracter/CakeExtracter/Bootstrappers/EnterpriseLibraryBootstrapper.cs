using System.ComponentModel.Composition;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace CakeExtracter.Bootstrappers
{
    [Export(typeof(IBootstrapper))]
    public class EnterpriseLibraryBootstrapper : IBootstrapper
    {
        public void Run()
        {
            InitializeLogging();
        }

        private static void InitializeLogging()
        {
            IConfigurationSource configurationSource = ConfigurationSourceFactory.Create();
            LogWriterFactory logWriterFactory = new LogWriterFactory(configurationSource);
            Microsoft.Practices.EnterpriseLibrary.Logging.Logger.SetLogWriter(logWriterFactory.Create());
        }
    }
}
