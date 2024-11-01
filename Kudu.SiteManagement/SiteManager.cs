﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Kudu.Client.Deployment;
using Kudu.Client.Infrastructure;
using Kudu.Contracts.Settings;
using Kudu.Contracts.SourceControl;
using Kudu.Core.Infrastructure;
using Kudu.SiteManagement.Certificates;
using Kudu.SiteManagement.Configuration;
using Kudu.SiteManagement.Configuration.Section;
using Kudu.SiteManagement.Context;
using Microsoft.Web.Administration;
using IIS = Microsoft.Web.Administration;

namespace Kudu.SiteManagement
{
    public class SiteManager : ISiteManager
    {
        private const string HostingStartHtml = "hostingstart.html";
        private const string HostingStartHtmlContents = @"<html>
<head>
<title>This web site is up and running</title>
<style type=""text/css"">
    BODY { color: #444444; background-color: #E5F2FF; font-family: verdana; margin: 0px; text-align: center; margin-top: 100px; }
    H1 { font-size: 16pt; margin-bottom: 4px; }
</style>
</head>
<body>
<h1>This web site is up and running</h1><br/>
</body> 
</html>";

        private readonly static Random portNumberGenRnd = new Random((int)DateTime.UtcNow.Ticks);

        private readonly string _logPath;
        private readonly bool _traceFailedRequests;
        private readonly IKuduContext _context;
        private readonly ICertificateSearcher _certificateSearcher;

        public SiteManager(IKuduContext context, ICertificateSearcher certificateSearcher)
            : this(context, certificateSearcher, false, null)
        {
        }

        public SiteManager(IKuduContext context, ICertificateSearcher certificateSearcher, bool traceFailedRequests, string logPath)
        {
            _logPath = logPath;
            _traceFailedRequests = traceFailedRequests;
            _context = context;
            _certificateSearcher = certificateSearcher;
        }

        public IEnumerable<string> GetSites()
        {
            using (ServerManager iis = GetServerManager())
            {
                try
                {
                    // The app pool is the app name
                    return iis.Sites.Where(x => x.Name.StartsWith("kudu_", StringComparison.OrdinalIgnoreCase))
                                    .Select(x => x.Applications[0].ApplicationPoolName)
                                    .Distinct()
                                    .ToList();
                }
                catch
                {
                    return new List<string>();
                }
            }
        }

        public Site GetSite(string applicationName)
        {
            using (ServerManager iis = GetServerManager())
            {
                string mainSiteName = GetLiveSite(applicationName);
                string serviceSiteName = GetServiceSite(applicationName);

                IIS.Site mainSite = iis.Sites[mainSiteName];

                if (mainSite == null)
                {
                    return null;
                }

                IIS.Site serviceSite = iis.Sites[serviceSiteName];
                // IIS.Site devSite = iis.Sites[devSiteName];

                Site site = new Site();
                site.ServiceBindings = GetSiteUrls(serviceSite);
                site.SiteBindings = GetSiteUrls(mainSite);
                return site;
            }
        }

        private IList<KuduBinding> GetSiteUrls(IIS.Site site)
        {
            if (site == null)
            {
                return null; 
            }
            return site.Bindings.Select(MapBinding).ToList();
        }

        private KuduBinding MapBinding(Binding binding)
        {
            KuduBinding kuduBinding = new KuduBinding();
            kuduBinding.Host = binding.Host;
            kuduBinding.Scheme = binding.Protocol.Equals("http", StringComparison.OrdinalIgnoreCase) ? UriScheme.Http : UriScheme.Https;
            kuduBinding.Port = binding.EndPoint.Port;
            kuduBinding.Ip = binding.EndPoint.Address.ToString();

            //NOTE: A KuduBinding also has information about certificate name etc...
            //      and SNI which we could try and fetch...

            //Extra for making target URLS.
            kuduBinding.DnsName = _context.HostName;
            return kuduBinding;
        }

        public async Task<Site> CreateSiteAsync(string applicationName)
        {
            using (ServerManager iis = GetServerManager())
            {
                try
                {
                    List<IBindingConfiguration> siteBindingCongfigs = new List<IBindingConfiguration>();
                    List<IBindingConfiguration> svcSiteBindingCongfigs = new List<IBindingConfiguration>();
                    if (_context.Configuration != null && _context.Configuration.Bindings != null)
                    {
                        siteBindingCongfigs = _context.Configuration.Bindings.Where(b => b.SiteType == SiteType.Live).ToList();
                        svcSiteBindingCongfigs = _context.Configuration.Bindings.Where(b => b.SiteType == SiteType.Service).ToList();
                    }

                    // Determine the host header values
                    List<BindingInformation> siteBindings = BuildDefaultBindings(applicationName, siteBindingCongfigs).ToList();
                    List<BindingInformation> serviceSiteBindings = BuildDefaultBindings(applicationName, svcSiteBindingCongfigs).ToList();

                    // Create the service site for this site
                    IIS.Site serviceSite = CreateSiteAsync(iis, applicationName, GetServiceSite(applicationName), _context.Configuration.ServiceSitePath, serviceSiteBindings);

                    // Create the main site
                    string siteName = GetLiveSite(applicationName);
                    string root = _context.Paths.GetApplicationPath(applicationName);
                    string siteRoot = _context.Paths.GetLiveSitePath(applicationName);
                    string webRoot = Path.Combine(siteRoot, Constants.WebRoot);

                    FileSystemHelpers.EnsureDirectory(webRoot);
                    File.WriteAllText(Path.Combine(webRoot, HostingStartHtml), HostingStartHtmlContents);

                    IIS.Site site = CreateSiteAsync(iis, applicationName, siteName, webRoot, siteBindings);

                    // Map a path called _app to the site root under the service site
                    MapServiceSitePath(iis, applicationName, Constants.MappedSite, root);

                    // Commit the changes to iis
                    iis.CommitChanges();

                    IList<KuduBinding> serviceBindings = GetSiteUrls(serviceSite);

                    // Wait for the site to start
                    await OperationManager.AttemptAsync(() => WaitForSiteAsync(serviceBindings.First().ToString()));

                    // Set initial ScmType state to LocalGit
                    ICredentials credentials = _context.Configuration.BasicAuthCredential.GetCredentials();
                    RemoteDeploymentSettingsManager settings = new RemoteDeploymentSettingsManager(serviceBindings.First() + "api/settings", credentials);
                    await settings.SetValue(SettingsKeys.ScmType, ScmType.LocalGit);

                    return new Site
                    {
                        ServiceBindings = serviceBindings,
                        SiteBindings = GetSiteUrls(site)
                    };
                }
                catch
                {
                    try
                    {
                        await DeleteSiteAsync(applicationName);
                    }
                    catch
                    {
                        // Don't let it throw if we're unable to delete a failed creation.
                    }
                    throw;
                }
            }
        }

        public async Task ResetSiteContent(string applicationName)
        {
            const int MaxWaitSeconds = 300;
            using (ServerManager iis = GetServerManager())
            {
                ApplicationPool appPool = iis.ApplicationPools.FirstOrDefault(ap => ap.Name == applicationName);
                if (appPool == null)
                {
                    throw new InvalidOperationException($"Failed to recycle {applicationName} app pool.  It does not exist!");
                }

                // the app pool is running or starting, so stop it first.
                if (appPool.State == ObjectState.Started || appPool.State == ObjectState.Starting)
                {
                    // wait for the app to finish before trying to stop
                    for (int i = 0; i > MaxWaitSeconds && appPool.State == ObjectState.Starting; ++i)
                    {
                        await Task.Delay(1000);
                    }

                    // stop the app if it isn't already stopped
                    if (appPool.State != ObjectState.Stopped)
                    {
                        appPool.Stop();
                    }
                }

                // wait for the app to stop
                for (int i = 0; appPool.State != ObjectState.Stopped; ++i)
                {
                    await Task.Delay(1000);
                    if (i > MaxWaitSeconds)
                    {
                        throw new InvalidOperationException($"Failed to recycle {applicationName} app pool.  Its state '{appPool.State}' is not stopped!");
                    }
                }

                string root = _context.Paths.GetApplicationPath(applicationName);
                string siteRoot = _context.Paths.GetLiveSitePath(applicationName);
                foreach (string dir in Directory.GetDirectories(root).Concat(new[] { Path.Combine(Path.GetTempPath(), applicationName) }))
                {
                    if (!Directory.Exists(dir))
                    {
                        continue;
                    }

                    // use rmdir command since it handles both hidden and read-only files
                    OperationManager.SafeExecute(() => Process.Start(new ProcessStartInfo
                    {
                        Arguments = $"/C rmdir /s /q \"{dir}\"",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        FileName = "cmd.exe"
                    })?.WaitForExit());

                    if (Directory.Exists(dir) && !dir.Contains(".deleted."))
                    {
                        string dirName = Path.GetFileName(dir);
                        OperationManager.Attempt(() => Process.Start(new ProcessStartInfo
                        {
                            Arguments = $"/C ren \"{dir}\" \"{dirName}.deleted.{Guid.NewGuid():N}\"",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            CreateNoWindow = true,
                            FileName = "cmd.exe"
                        })?.WaitForExit());
                    }
                }

                string webRoot = Path.Combine(siteRoot, Constants.WebRoot);
                FileSystemHelpers.EnsureDirectory(siteRoot);
                FileSystemHelpers.EnsureDirectory(webRoot);
                File.WriteAllText(Path.Combine(webRoot, HostingStartHtml), HostingStartHtmlContents);

                // start the app
                appPool.Start();

                // wait for the app to stop
                for (int i = 0; appPool.State != ObjectState.Started; ++i)
                {
                    await Task.Delay(1000);
                    if (i > MaxWaitSeconds)
                    {
                        throw new InvalidOperationException($"Failed to recycle {applicationName} app pool.  Its state '{appPool.State}' is not started!");
                    }
                }
            }
        }

        //NOTE: Small temporary object for configuration.
        private struct BindingInformation
        {
            public string Binding { get; set; }
            public IBindingConfiguration Configuration { get; set; }
            //public string Url { get { return Configuration.Url; } }
            public UriScheme Scheme { get { return Configuration.Scheme; } }
            //public SiteType SiteType { get { return Configuration.SiteType; } }
            public string Certificate { get { return Configuration.Certificate; } }
        }

        private static IEnumerable<BindingInformation> BuildDefaultBindings(string applicationName, IEnumerable<IBindingConfiguration> bindings)
        {
            return bindings.Select(configuration => configuration.Scheme == UriScheme.Http
                ? new BindingInformation { Configuration = configuration, Binding = CreateBindingInformation(applicationName, configuration.Url) }
                : new BindingInformation { Configuration = configuration, Binding = CreateBindingInformation(applicationName, configuration.Url, defaultPort: "443") })
                //NOTE: We order the bindings so we get the http bindings on top, this means we can easily prioritise those for testing site setup later.
                .OrderBy(b => b.Scheme);
        }

        public async Task DeleteSiteAsync(string applicationName)
        {
            string appPoolName = GetAppPool(applicationName);
            using (ServerManager iis = GetServerManager())
            {
                // Get the app pool for this application
                ApplicationPool kuduPool = iis.ApplicationPools[appPoolName];

                if (kuduPool == null)
                {
                    // If there's no app pool then do nothing
                    return;
                }

                await Task.WhenAll(
                    DeleteSiteAsync(iis, GetLiveSite(applicationName)),
                    // Don't delete the physical files for the service site
                    DeleteSiteAsync(iis, GetServiceSite(applicationName), deletePhysicalFiles: false)
                    );


                try
                {
                    iis.CommitChanges();
                    Thread.Sleep(1000);
                }
                catch (NotImplementedException)
                {
                    //NOTE: For some reason, deleting a site with a HTTPS bindings results in a NotImplementedException on Windows 7, but it seems to remove everything relevant anyways.
                }
            }

            //NOTE: DeleteSiteAsync was not split into to usings before, but by calling CommitChanges midway, the iis manager goes into a read-only mode on Windows7 which then provokes
            //      an error on the next commit. On the next pass. Acquirering a new Manager seems like a more safe approach.
            using (ServerManager iis = GetServerManager())
            {
                string appPath = _context.Paths.GetApplicationPath(applicationName);
                string sitePath = _context.Paths.GetLiveSitePath(applicationName);
                try
                {
                    DeleteSafe(sitePath);
                    DeleteSafe(appPath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
                finally
                {

                    iis.ApplicationPools.Remove(iis.ApplicationPools[appPoolName]);
                    iis.CommitChanges();

                    // Clear out the app pool user profile directory if it exists
                    string userDir = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).TrimEnd(Path.DirectorySeparatorChar));
                    string appPoolDirectory = Path.Combine(userDir, appPoolName);
                    DeleteSafe(appPoolDirectory);
                }
            }
        }

        public bool AddSiteBinding(string applicationName, KuduBinding binding)
        {
            try
            {
                using (ServerManager iis = GetServerManager())
                {
                    if (!IsAvailable(binding.Host, binding.Port, iis))
                    {
                        return false;
                    }

                    IIS.Site site = binding.SiteType == SiteType.Live
                        ? iis.Sites[GetLiveSite(applicationName)]
                        : iis.Sites[GetServiceSite(applicationName)];

                    if (site == null)
                    {
                        return true;
                    }

                    string bindingInformation = string.Format("{0}:{1}:{2}", binding.Ip, binding.Port, binding.Host);
                    switch (binding.Scheme)
                    {
                        case UriScheme.Http:
                            site.Bindings.Add(bindingInformation, "http");
                            break;

                        case UriScheme.Https:
                            Certificate cert = _certificateSearcher.Lookup(binding.Certificate).ByThumbprint();
                            Binding bind = site.Bindings.Add(bindingInformation, cert.GetCertHash(), cert.StoreName);
                            if (binding.Sni)
                            {
                                bind.SetAttributeValue("sslFlags", SslFlags.Sni);
                            }

                            break;
                    }
                    iis.CommitChanges();
                    Thread.Sleep(1000);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public bool RemoveSiteBinding(string applicationName, KuduBinding siteBinding, SiteType siteType)
        {
            try
            {
                using (ServerManager iis = GetServerManager())
                {
                    IIS.Site site = siteType == SiteType.Live
                        ? iis.Sites[GetLiveSite(applicationName)]
                        : iis.Sites[GetServiceSite(applicationName)];

                    if (site == null)
                    { return true; }

                    Binding binding = site.Bindings
                        .FirstOrDefault(x => x.Host.Equals(siteBinding.Host)
                            && x.EndPoint.Port.Equals(siteBinding.Port)
                            && x.EndPoint.Address.ToString() == siteBinding.Ip
                            && x.Protocol.Equals(siteBinding.Scheme.ToString(), StringComparison.OrdinalIgnoreCase));

                    if (binding == null)
                    { return true; }

                    site.Bindings.Remove(binding);
                    iis.CommitChanges();
                    Thread.Sleep(1000);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        private static void MapServiceSitePath(ServerManager iis, string applicationName, string path, string siteRoot)
        {
            string serviceSiteName = GetServiceSite(applicationName);

            // Get the service site
            IIS.Site site = iis.Sites[serviceSiteName];
            if (site == null)
            {
                throw new InvalidOperationException("Could not retrieve service site");
            }

            // Map the path to the live site in the service site
            site.Applications.Add(path, siteRoot);
        }

        private static ApplicationPool EnsureAppPool(ServerManager iis, string appName)
        {
            string appPoolName = GetAppPool(appName);
            ApplicationPool kuduAppPool = iis.ApplicationPools[appPoolName];
            if (kuduAppPool == null)
            {
                iis.ApplicationPools.Add(appPoolName);
                iis.CommitChanges();
                kuduAppPool = iis.ApplicationPools[appPoolName];
                kuduAppPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                kuduAppPool.ManagedRuntimeVersion = "v4.0";
                kuduAppPool.AutoStart = true;
                kuduAppPool.ProcessModel.LoadUserProfile = true;
                kuduAppPool.Failure.RapidFailProtection = false;

                // We've seen strange errors after switching to VS 2015 msbuild when using App Pool Identity.
                // The errors look like:
                // error CS0041 : Unexpected error writing debug information -- 'Retrieving the COM class factory for component with CLSID {0AE2DEB0-F901-478B-BB9F-881EE8066788} failed due to the following error : 800703fa Illegal operation attempted on a registry key that has been marked for deletion. (Exception from HRESULT: 0x800703FA).'
                // To work around this, we're using NetworkService. But it would be good to understand the root
                // cause of the issue.
                if (ConfigurationManager.AppSettings["UseNetworkServiceIdentity"] == "true")
                {
                    kuduAppPool.ProcessModel.IdentityType = ProcessModelIdentityType.NetworkService;
                }
            }

            EnsureDefaultDocument(iis);

            return kuduAppPool;
        }


        private static int GetRandomPort(ServerManager iis)
        {
            int randomPort = portNumberGenRnd.Next(1025, 65535);
            while (!IsAvailable(randomPort, iis))
            {
                randomPort = portNumberGenRnd.Next(1025, 65535);
            }

            return randomPort;
        }

        private static bool IsAvailable(int port, ServerManager iis)
        {
            TcpConnectionInformation[] tcpConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
            return tcpConnections.All(connectionInfo => connectionInfo.LocalEndPoint.Port != port)
                && iis.Sites
                    .SelectMany(iisSite => iisSite.Bindings)
                    .All(binding => binding.EndPoint == null || binding.EndPoint.Port != port);
        }

        private static bool IsAvailable(string host, int port, ServerManager iis)
        {
            return iis.Sites
                .SelectMany(iisSite => iisSite.Bindings)
                .All(binding => binding.EndPoint == null || binding.EndPoint.Port != port || binding.Host != host);
        }

        private IIS.Site CreateSiteAsync(ServerManager iis, string applicationName, string siteName, string siteRoot, List<SiteManager.BindingInformation> bindings)
        {
            ApplicationPool pool = EnsureAppPool(iis, applicationName);

            IIS.Site site;
            if (bindings.Any())
            {
                BindingInformation first = bindings.First();

                //Binding primaryBinding;
                site = first.Scheme == UriScheme.Http
                    ? iis.Sites.Add(siteName, "http", first.Binding, siteRoot)
                    : iis.Sites.Add(siteName, first.Binding, siteRoot, _certificateSearcher.Lookup(first.Certificate).ByFriendlyName().GetCertHash());
                if (first.Configuration.RequireSni)
                {
                    site.Bindings.First().SetAttributeValue("sslFlags", SslFlags.Sni);
                }

                //Note: Add the rest of the bindings normally.
                foreach (BindingInformation binding in bindings.Skip(1))
                {
                    switch (binding.Scheme)
                    {
                        case UriScheme.Http:
                            site.Bindings.Add(binding.Binding, "http");
                            break;

                        case UriScheme.Https:
                            Certificate cert = _certificateSearcher.Lookup(binding.Certificate).ByFriendlyName();
                            if (cert == null)
                            {
                                throw new ConfigurationErrorsException(string.Format("Could not find a certificate by the name '{0}'.", binding.Certificate));
                            }

                            Binding bind = site.Bindings.Add(binding.Binding, cert.GetCertHash(), cert.StoreName);
                            if (binding.Configuration.RequireSni)
                            {
                                bind.SetAttributeValue("sslFlags", SslFlags.Sni);
                            }
                            break;
                    }
                }
            }
            else
            {
                site = iis.Sites.Add(siteName, siteRoot, GetRandomPort(iis));
            }

            site.ApplicationDefaults.ApplicationPoolName = pool.Name;

            if (!_traceFailedRequests)
                return site;

            site.TraceFailedRequestsLogging.Enabled = true;
            string path = Path.Combine(_logPath, applicationName, "Logs");
            Directory.CreateDirectory(path);
            site.TraceFailedRequestsLogging.Directory = path;

            return site;
        }

        private static void EnsureDefaultDocument(ServerManager iis)
        {
            IIS.Configuration applicationHostConfiguration = iis.GetApplicationHostConfiguration();
            IIS.ConfigurationSection defaultDocumentSection = applicationHostConfiguration.GetSection("system.webServer/defaultDocument");

            IIS.ConfigurationElementCollection filesCollection = defaultDocumentSection.GetCollection("files");

            if (filesCollection.Any(ConfigurationElementContainsHostingStart))
                return;

            IIS.ConfigurationElement addElement = filesCollection.CreateElement("add");
            addElement["value"] = HostingStartHtml;
            filesCollection.Add(addElement);

            iis.CommitChanges();
        }

        private static bool ConfigurationElementContainsHostingStart(IIS.ConfigurationElement configurationElement)
        {
            object valueAttribute = configurationElement["value"];

            return valueAttribute != null && String.Equals(HostingStartHtml, valueAttribute.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static string CreateBindingInformation(string applicationName, string baseUrl, string defaultIp = "*", string defaultPort = "80")
        {
            // Creates the 'bindingInformation' parameter for IIS.ServerManager.Sites.Add()
            // Accepts baseUrl in 3 formats: hostname, hostname:port and ip:port:hostname

            // Based on the default parameters, applicationName + baseUrl it creates
            // a string in the format ip:port:hostname

            string[] parts = baseUrl.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            string ip = defaultIp;
            string host = string.Empty;
            string port = defaultPort;

            switch (parts.Length)
            {
                case 1: // kudu.mydomain
                    host = parts[0];
                    break;
                case 2: // kudu.mydomain:8080
                    host = parts[0];
                    port = parts[1];
                    break;
                case 3: // 192.168.100.3:80:kudu.mydomain
                    ip = parts[0];
                    port = parts[1];
                    host = parts[2];
                    break;
            }

            return string.Format("{0}:{1}:{2}", ip, port, applicationName + "." + host);
        }

        private static Task DeleteSiteAsync(ServerManager iis, string siteName, bool deletePhysicalFiles = true)
        {
            IIS.Site site = iis.Sites[siteName];
            if (site != null)
            {
                return OperationManager.AttemptAsync(async () =>
                {
                    await Task.Run(() =>
                    {
                        if (deletePhysicalFiles)
                        {
                            string physicalPath = site.Applications[0].VirtualDirectories[0].PhysicalPath;
                            DeleteSafe(physicalPath);
                        }
                        iis.Sites.Remove(site);
                    });
                });
            }

            return Task.FromResult(0);
        }

        private static string GetLiveSite(string applicationName)
        {
            return "kudu_" + applicationName;
        }

        private static string GetServiceSite(string applicationName)
        {
            return "kudu_service_" + applicationName;
        }

        private static string GetAppPool(string applicationName)
        {
            return applicationName;
        }

        private static void DeleteSafe(string physicalPath)
        {
            if (!Directory.Exists(physicalPath))
            {
                return;
            }

            FileSystemHelpers.DeleteDirectorySafe(physicalPath);
        }

        private ServerManager GetServerManager()
        {
            return new ServerManager(_context.Configuration.IISConfigurationFile);
        }

        private async Task WaitForSiteAsync(string serviceUrl)
        {
            ICredentials credentials = _context.Configuration.BasicAuthCredential.GetCredentials();
            using (HttpClient client = HttpClientHelper.CreateClient(serviceUrl, credentials))
            {
                using (HttpResponseMessage response = await client.GetAsync(serviceUrl))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}