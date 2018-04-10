[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(DirectAgents.Web.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(DirectAgents.Web.App_Start.NinjectWebCommon), "Stop")]

namespace DirectAgents.Web.App_Start
{
    using System;
    using System.Web;
    using DirectAgents.Domain.Abstract;
    using DirectAgents.Domain.Concrete;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<ISecurityRepository>().To<SecurityRepository>();
            kernel.Bind<IMainRepository>().To<MainRepository>();
            kernel.Bind<ICPProgRepository>().To<CPProgRepository>();
            kernel.Bind<ICPSearchRepository>().To<CPSearchRepository>();
            kernel.Bind<ITDRepository>().To<TDRepository>();
            kernel.Bind<IRevTrackRepository>().To<RevTrackRepository>();
            kernel.Bind<IABRepository>().To<ABRepository>();
            kernel.Bind<ISuperRepository>().To<SuperRepository>();

            kernel.Bind<ClientPortal.Data.Contracts.IClientPortalRepository>().To<ClientPortal.Data.Services.ClientPortalRepository>();
        }        
    }
}
