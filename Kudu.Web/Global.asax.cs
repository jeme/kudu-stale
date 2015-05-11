using System.Data.Entity;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Kudu.Web.Models;
using System.Diagnostics.CodeAnalysis;

namespace Kudu.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        private static void RegisterDefaultRoute(RouteCollection routes)
        {
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{slug}", // URL with parameters
                new { controller = "Application", action = "Index", slug = UrlParameter.Optional } // Parameter defaults
            );
        }

        public static void RegisterViewRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Deployments",
                            "deployments/{slug}",
                            new { controller = "Deployments", action = "Index" });

            routes.MapRoute("TriggerFetch",
                            "deployments/{slug}/trigger-fetch",
                            new { controller = "Deployments", action = "TriggerFetch" },
                            new { verb = new HttpMethodConstraint("POST") });

            routes.MapRoute("Deploy",
                            "deployments/{slug}/deploy/{id}",
                            new { controller = "Deployments", action = "Deploy" });

            routes.MapRoute("DeploymentLog",
                            "deployments/{slug}/log/{id}",
                            new { controller = "Deployments", action = "Log" });

            routes.MapRoute("DeploymentLogVerbose",
                            "deployments/{slug}/log/{id}/{logId}",
                            new { controller = "Deployments", action = "Details" });

            routes.MapRoute("ApplicationCreate",
                            "application/create",
                            new { controller = "Application", action = "Create" });

            routes.MapRoute("ApplicationDetails",
                            "application/{slug}",
                            new { controller = "Application", action = "Details" });

            routes.MapRoute("Configuration",
                            "configuration/{slug}",
                            new { controller = "Settings", action = "Index" });
        }

        private static void RegisterApiRoutes(RouteCollection routes)
        {
            routes.MapHttpRoute("Application-API-Route",
                        "api/application/{slug}/{action}",
                        new { controller = "Application", action = "Get" });

            routes.MapHttpRoute("Applications-API-Route",
                                "api/applications/{action}/{slug}",
                                new { controller = "Applications", action = "All", slug= RouteParameter.Optional });
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "By design")]
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterViewRoutes(RouteTable.Routes);
            RegisterApiRoutes(RouteTable.Routes);
            RegisterDefaultRoute(RouteTable.Routes);
        }

    }
}