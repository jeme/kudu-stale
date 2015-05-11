using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using Kudu.Client.Infrastructure;
using Kudu.SiteManagement;
using Kudu.SiteManagement.Certificates;
using Kudu.SiteManagement.Configuration;
using Kudu.SiteManagement.Context;
using Kudu.Web.Infrastructure;
using Kudu.Web.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Ninject;
using Ninject.Web.Common;
using WebApiContrib.IoC.Ninject;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Kudu.Web.App_Start.Startup), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(Kudu.Web.App_Start.Startup), "Stop")]

namespace Kudu.Web.App_Start
{
    public static class Startup
    {
        private static readonly Bootstrapper _bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            HttpApplication.RegisterModule(typeof(OnePerRequestHttpModule));
            HttpApplication.RegisterModule(typeof(NinjectHttpModule));
            _bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            _bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();

            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(kernel);
            SetupWebAip(kernel);
            return kernel;
        }

        private static void SetupWebAip(StandardKernel kernel)
        {
            var configuration = GlobalConfiguration.Configuration;
            configuration.DependencyResolver = new NinjectResolver(kernel);
            configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            SetupKuduServices(kernel);
        }

        private static void SetupKuduServices(IKernel kernel)
        {
            IKuduConfiguration configuration = KuduConfiguration.Load(HttpRuntime.AppDomainAppPath);
            kernel.Bind<IKuduConfiguration>().ToConstant(configuration);
            kernel.Bind<IPathResolver>().To<PathResolver>();
            kernel.Bind<ISiteManager>().To<SiteManager>().InSingletonScope();
            kernel.Bind<ICertificateSearcher>().To<CertificateSearcher>();
            kernel.Bind<IKuduContext>().To<KuduContext>();

            //TODO: Instantialte from container instead of factory.
            kernel.Bind<KuduEnvironment>().ToMethod(_ => new KuduEnvironment
            {
                RunningAgainstLocalKuduService = true,
                IsAdmin = IdentityHelper.IsAnAdministrator(),
                ServiceSitePath = configuration.ServiceSitePath,
                SitesPath = configuration.ApplicationsPath
            });


            // TODO: Integrate with membership system
            kernel.Bind<ICredentialProvider>().ToConstant(new BasicAuthCredentialProvider("admin", "kudu"));
            kernel.Bind<IApplicationService>().To<ApplicationService>().InRequestScope();
            kernel.Bind<ISettingsService>().To<SettingsService>();

            // Sql CE setup
            Directory.CreateDirectory(Path.Combine(configuration.RootPath, "App_Data"));
        }
    }
}
