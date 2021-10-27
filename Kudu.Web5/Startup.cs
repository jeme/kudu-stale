using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kudu.Client.Infrastructure;
using Kudu.Services;
using Kudu.SiteManagement;
using Kudu.SiteManagement.Certificates;
using Kudu.SiteManagement.Configuration;
using Kudu.SiteManagement.Context;
using Kudu.Web.Infrastructure;
using Kudu.Web.Models;
using Kudu.Web5.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using ISettingsService = Kudu.Web5.Services.ISettingsService;

namespace Kudu.Web5
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                JsonConvert.DefaultSettings = ()=>options.SerializerSettings;
            });
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "app/dist";
            });

            services.Configure<KuduSettings>(settings => Configuration.GetSection(nameof(KuduSettings)).Bind(settings));
            //IKuduConfiguration configuration = KuduConfiguration.Load(HttpRuntime.AppDomainAppPath);
            services.AddSingleton<IKuduConfiguration, KuduConfigurationJsonAdaptor>();
            services.AddSingleton<ISiteManager, SiteManager>();
            services.AddSingleton<ICredentialProvider>(provider =>
            {
                var config = provider.GetRequiredService<IKuduConfiguration>();
                return config.BasicAuthCredential;
            });
            services.AddSingleton<KuduEnvironment>(provider =>
            {
                var config = provider.GetRequiredService<IKuduConfiguration>();
                return new KuduEnvironment()
                {
                    RunningAgainstLocalKuduService = true,
                    IsAdmin = IdentityHelper.IsAnAdministrator(),
                    ServiceSitePath = config.ServiceSitePath,
                    SitesPath = config.ApplicationsPath
                };
            });

            services.AddTransient<IPathResolver, PathResolver>();
            services.AddTransient<ICertificateSearcher, CertificateSearcher>();
            services.AddTransient<IKuduContext, KuduContext>();

            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddScoped<IWeb5SettingsService, DefaultSettingsService>();
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
                    //spa.UseAngularCliServer(npmScript: "start");
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }

    public class KuduConfigurationJsonAdaptor : IKuduConfiguration
    {
        private const string DefaultIisConfigurationFile = "%windir%\\system32\\inetsrv\\config\\applicationHost.config";
   
        private readonly IWebHostEnvironment _env;
        private readonly IOptions<KuduSettings> _settings;
        public string RootPath => _env.WebRootPath;

        public KuduConfigurationJsonAdaptor(IOptions<KuduSettings> settings, IWebHostEnvironment env)
        {
            _settings = settings;
            _env = env;
        }

        public bool CustomHostNamesEnabled => _settings.Value.EnableCustomHostNames;

        public string ApplicationsPath => PathRelativeToRoot(_settings.Value.ApplicationsPath);
        public string ServiceSitePath => PathRelativeToRoot(_settings.Value.ServiceSitePath);

        private string PathRelativeToRoot(string path)
        {
            string combined = Path.Combine(RootPath, path);
            return Path.GetFullPath(combined);
        }

        public string IISConfigurationFile => _settings.Value.IisConfigurationFile ?? Environment.ExpandEnvironmentVariables(DefaultIisConfigurationFile);
        public BasicAuthCredentialProvider BasicAuthCredential => new BasicAuthCredentialProvider("", "");
        public IEnumerable<IBindingConfiguration> Bindings { get; }
        public IEnumerable<ICertificateStoreConfiguration> CertificateStores { get; }
    }

    public class KuduSettings
    {
        public string IisConfigurationFile { get; set; }
        public bool EnableCustomHostNames { get; set; }

        public string ServiceSitePath { get; set; }
        public string ApplicationsPath { get; set; }

        public KuduBindingsSettings Bindings { get; set; }
        public CertificateStore[] CertificateStores { get; set; }
    }

    public class KuduBindingsSettings
    {
        public KuduBindingSettings[] Application { get; set; }
        public KuduBindingSettings[] Service { get; set; }
    }

    public class KuduBindingSettings
    {
        public string Scheme { get; set; }
        public string Url { get; set; }
        public string Certificate { get; set; }
        public bool RequiresSni { get; set; }
    }

    public class CertificateStore
    {
        public string Name { get; set; }
    }

    /*
"KuduManagement": {
    "IisConfigurationFile": "c:\\myconfig.config",
    "EnableCustomHostNames": false,
    "ServiceSitePath": "..\\Kudu.Services.Web",
    "ApplicationsPath": "..\\apps",
    "Bindings": {
      "Application": [
        {
          "Scheme": "http",
          "Url": "kudu.localtest.me"
        },
        {
          "Scheme": "https",
          "Url": "kudu.localtest.me",
          "Certificate": "*.localtest.me",
          "RequiresSni": true
        }
      ],
      "Service": [
        {
          "Scheme": "http",
          "Url": "scm.kudu.localtest.me"
        },
        {
          "Scheme": "https",
          "Url": "scm.kudu.localtest.me",
          "Certificate": "*.localtest.me",
          "RequiresSni": true
        }
      ]
    },
    "CertificateStores": [
      { "Name": "My" },
      { "Name": "Root" }
    ]
  }     *
     *
     */

}
