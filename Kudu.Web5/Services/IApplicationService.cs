using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kudu.SiteManagement;

namespace Kudu.Web5.Services
{

    public interface IApplication
    {
        string Name { get; set; }
        KuduBinding PrimarySiteBinding { get; }
        KuduBinding PrimaryServiceBinding { get; }

        IList<KuduBinding> SiteBindings { get; set; }
        IList<KuduBinding> ServiceBindings { get; set; }
    }

    public class Application : IApplication
    {

        public string Name { get; set; }

        public KuduBinding PrimaryServiceBinding => ServiceBindings.First();
        public KuduBinding PrimarySiteBinding => SiteBindings.First();

        public IList<KuduBinding> SiteBindings { get; set; }
        public IList<KuduBinding> ServiceBindings { get; set; }
        public Application()
        {
            SiteBindings = new List<KuduBinding>();
            ServiceBindings = new List<KuduBinding>();
        }
    }

    public interface IApplicationService
    {
        Task AddApplication(string name);
        Task<bool> DeleteApplication(string name);
        IEnumerable<string> GetApplications();
        IApplication GetApplication(string name);
        bool RemoveLiveSiteBinding(string name, KuduBinding siteBinding);
        bool RemoveServiceSiteBinding(string name, KuduBinding siteBinding);
        bool AddSiteBinding(string name, KuduBinding binding);
    }
    public class ApplicationService : IApplicationService
    {
        private readonly ISiteManager _siteManager;

        public ApplicationService(ISiteManager siteManager)
        {
            _siteManager = siteManager;
        }

        public Task AddApplication(string name)
        {
            if (GetApplications().Any(x => x == name))
            {
                throw new SiteExistsException();
            }

            return _siteManager.CreateSiteAsync(name);
        }

        public async Task<bool> DeleteApplication(string name)
        {
            var application = GetApplication(name);
            if (application == null)
            {
                return false;
            }

            await _siteManager.DeleteSiteAsync(name);
            return true;
        }

        public IEnumerable<string> GetApplications()
        {
            return _siteManager.GetSites();
        }

        public IApplication GetApplication(string name)
        {
            Site site = _siteManager.GetSite(name);
            if (site == null)
            {
                throw new SiteNotFoundException();
            }

            return new Application
            {
                Name = name,
                SiteBindings = site.SiteBindings,
                ServiceBindings = site.ServiceBindings
            };
        }

        public bool RemoveLiveSiteBinding(string name, KuduBinding siteBinding)
        {
            var application = GetApplication(name);
            if (application == null)
            {
                return false;
            }

            return _siteManager.RemoveSiteBinding(name, siteBinding, SiteType.Live);
        }

        public bool RemoveServiceSiteBinding(string name, KuduBinding siteBinding)
        {
            var application = GetApplication(name);
            if (application == null)
            {
                return false;
            }

            return _siteManager.RemoveSiteBinding(name, siteBinding, SiteType.Service);
        }

        public bool AddSiteBinding(string name, KuduBinding binding)
        {
            var application = GetApplication(name);
            if (application == null)
            {
                return false;
            }

            return _siteManager.AddSiteBinding(name, binding);
        }
    }

    public class SiteExistsException : InvalidOperationException
    {
    }

    public class SiteNotFoundException : InvalidOperationException
    {
    }


}