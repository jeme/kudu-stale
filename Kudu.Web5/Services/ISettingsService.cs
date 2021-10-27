using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using Kudu.Client.Deployment;
using Kudu.Client.Infrastructure;
using Kudu.Web5.Infrastructure;

namespace Kudu.Web5.Services
{
    public interface IKuduSettings : IDictionary<string, string>
    {

    }

    public class KuduSettings : Dictionary<string, string>, IKuduSettings
    {
       
    }


    public interface ISettingsService
    {
        Task<IKuduSettings> Get(string siteName);
        Task<IKuduSettings> Set(string siteName, IKuduSettings settings);
    }

    public class SettingsService : ISettingsService
    {
        private readonly IApplicationService _applicationService;
        private readonly ICredentialProvider _credentialProvider;

        public SettingsService(IApplicationService applicationService, ICredentialProvider credentialProvider)
        {
            _applicationService = applicationService;
            _credentialProvider = credentialProvider;
        }

        public async Task<IKuduSettings> Get(string siteName)
        {
            RemoteDeploymentSettingsManager settingsManager = GetSettingsManager(siteName);
            NameValueCollection values = await settingsManager.GetValues();
            KuduSettings settings = new KuduSettings();
            foreach (string? key in values.AllKeys)
            {
                if (key != null)
                {
                    settings.Add(key, values.Get(key));
                }
            }
            return settings;
        }

        public Task<IKuduSettings> Set(string siteName, IKuduSettings settings)
        {
            throw new System.NotImplementedException();
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
