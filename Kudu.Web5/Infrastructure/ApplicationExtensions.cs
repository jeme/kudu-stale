using System.Net;
using System.Threading.Tasks;
using Kudu.Client.Deployment;
using Kudu.Client.SourceControl;
using Kudu.Core.SourceControl;
using Kudu.Web5.Services;

namespace Kudu.Web5.Infrastructure
{
    public static class ApplicationExtensions
    {
        public static Task<RepositoryInfo> GetRepositoryInfo(this IApplication application, ICredentials credentials)
        {
            var repositoryManager = new RemoteRepositoryManager(application.PrimaryServiceBinding + "api/scm", credentials);
            return repositoryManager.GetRepositoryInfo();
        }

        public static RemoteDeploymentManager GetDeploymentManager(this IApplication application, ICredentials credentials)
        {
            var deploymentManager = new RemoteDeploymentManager(application.PrimaryServiceBinding + "api", credentials);
            return deploymentManager;
        }

        public static RemoteFetchManager GetFetchManager(this IApplication application, ICredentials credentials)
        {
            return new RemoteFetchManager(application.PrimaryServiceBinding + "deploy", credentials);
        }

        public static RemoteDeploymentSettingsManager GetSettingsManager(this IApplication application, ICredentials credentials)
        {
            var deploymentSettingsManager = new RemoteDeploymentSettingsManager(application.PrimaryServiceBinding + "api/settings", credentials);
            return deploymentSettingsManager;
        }
    }
}
