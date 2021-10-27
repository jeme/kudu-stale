using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using Kudu.Client.Deployment;
using Kudu.Client.Infrastructure;
using Kudu.Web.Infrastructure;
using Kudu.Web.Models;

namespace Kudu.Web5.Services
{
    public interface ISettings
    {
        IDictionary<string, string> KuduSettings { get; }
        IDictionary<string, string> AppSettings { get; }
        IDictionary<string, string> ConnectionStrings { get; }
    }

    public class Settings : ISettings
    {
        public IDictionary<string, string> KuduSettings { get; } = new Dictionary<string, string>();
        public IDictionary<string, string> AppSettings { get; } = new Dictionary<string, string>();
        public IDictionary<string, string> ConnectionStrings { get; } = new Dictionary<string, string>();
    }


    public interface IWeb5SettingsService
    {
        Task<ISettings> GetSettings(string siteName);
        void SetConnectionString(string siteName, string name, string connectionString);
        void RemoveConnectionString(string siteName, string name);
        void RemoveAppSetting(string siteName, string key);
        void SetAppSetting(string siteName, string key, string value);
        Task SetKuduSetting(string siteName, string key, string value);
        Task SetKuduSettings(string siteName, params KeyValuePair<string, string>[] values);
        Task RemoveKuduSetting(string siteName, string key);
    }

    public class DefaultSettingsService : IWeb5SettingsService
    {
        private readonly IApplicationService _applicationService;
        private readonly ICredentialProvider _credentialProvider;

        public DefaultSettingsService(IApplicationService applicationService, ICredentialProvider credentialProvider)
        {
            _applicationService = applicationService;
            _credentialProvider = credentialProvider;
        }

        public async Task<ISettings> GetSettings(string siteName)
        {
            RemoteDeploymentSettingsManager settingsManager = GetSettingsManager(siteName);
            NameValueCollection values = await settingsManager.GetValues();
            Settings settings = new Settings();
            foreach (string? key in values.AllKeys)
            {
                if (key != null)
                {
                    settings.KuduSettings.Add(key, values.Get(key));
                }
            }
            return settings;
        }

        public void SetConnectionString(string siteName, string name, string connectionString)
        {
            // Not supported
        }

        public void RemoveConnectionString(string siteName, string name)
        {
            // Not supported
        }

        public void RemoveAppSetting(string siteName, string key)
        {
            // Not supported 
        }

        public void SetAppSetting(string siteName, string key, string value)
        {
            // Not supported
        }

        public Task SetKuduSetting(string siteName, string key, string value)
        {
            return GetSettingsManager(siteName).SetValue(key, value);
        }

        public Task SetKuduSettings(string siteName, params KeyValuePair<string, string>[] settings)
        {
            return GetSettingsManager(siteName).SetValues(settings);
        }

        public Task RemoveKuduSetting(string siteName, string key)
        {
            return GetSettingsManager(siteName).Delete(key);
        }

        protected RemoteDeploymentSettingsManager GetSettingsManager(string siteName)
        {
            IApplication application = _applicationService.GetApplication(siteName);
            ICredentials credentials = _credentialProvider.GetCredentials();
            RemoteDeploymentSettingsManager settingsManager = application.GetSettingsManager(credentials);
            return settingsManager;
        }
    }


}
