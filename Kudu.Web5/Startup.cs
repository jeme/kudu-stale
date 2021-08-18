using Kudu.SiteManagement;
using Kudu.SiteManagement.Certificates;
using Kudu.SiteManagement.Configuration;
using Kudu.SiteManagement.Context;
using Kudu.Web.Infrastructure;
using Kudu.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kudu.Web5
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "app/dist";
            });

            //IKuduConfiguration configuration = KuduConfiguration.Load(HttpRuntime.AppDomainAppPath);
            //services.AddSingleton<IKuduConfiguration>((IKuduConfiguration)null);
            services.AddSingleton<ISiteManager, SiteManager>();
            
            services.AddTransient<IPathResolver, PathResolver>();
            services.AddTransient<ICertificateSearcher, CertificateSearcher>();
            services.AddTransient<IKuduContext, KuduContext>();

            services.AddScoped<IApplicationService, ApplicationService>();

            services.AddSingleton<KuduEnvironment>(provider =>
            {
                return new KuduEnvironment()
                {
                    RunningAgainstLocalKuduService = true,
                    IsAdmin = IdentityHelper.IsAnAdministrator(),
                    ServiceSitePath = "",//Configuration.ServiceSitePath,
                    SitesPath = "", //Configuration.ApplicationsPath

                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "app";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
