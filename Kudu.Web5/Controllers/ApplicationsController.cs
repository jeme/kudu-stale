using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Kudu.Client.Infrastructure;
using Kudu.SiteManagement.Certificates;
using Kudu.SiteManagement.Context;
using Kudu.Web5.Infrastructure;
using Kudu.Web5.Services;
using Newtonsoft.Json.Linq;

namespace Kudu.Web5.Controllers
{
    [ApiController]
    [Route("api/v1/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly KuduEnvironment _environment;
        private readonly IKuduContext _context;
        private readonly IApplicationService _applicationService;
        private readonly ICertificateSearcher _certificates;
        private readonly ICredentialProvider _credentialProvider;
        private readonly ISettingsService _settingsService;

        public SettingsController(
            KuduEnvironment environment,
            IKuduContext context,
            IApplicationService applicationService,
            ICertificateSearcher certificates,
            ICredentialProvider credentialProvider,
            ISettingsService settingsService)
        {
            _environment = environment;
            _context = context;
            _applicationService = applicationService;
            _certificates = certificates;
            _credentialProvider = credentialProvider;
            _settingsService = settingsService;
        }

        [HttpGet]
        [Route("{name}")]
        public async Task<object> Get(string name)
        {
            IApplication app = _applicationService.GetApplication(name);
            if (app == null)
                return NotFound();

            return JObject.FromObject(await _settingsService.Get(name));
        }

        [HttpPut]
        [Route("{name}")]
        public async Task<object> Get(string name, JObject settings)
        {
            IApplication app = _applicationService.GetApplication(name);
            if (app == null)
                return NotFound();

            return JObject.FromObject(await _settingsService.Get(name));

            //ICredentials credentials = _credentialProvider.GetCredentials();
            //JObject entity = JObject.FromObject(app);
            //entity["repository"] = JObject.FromObject(await app.GetRepositoryInfo(credentials));
            //entity["settings"] = JObject.FromObject(await _settingsService.GetSettings(name));
            //return entity;
        }
    }

    [ApiController]
    [Route("api/v1/applications")]
    public class ApplicationsController : ControllerBase
    {
        private readonly KuduEnvironment _environment;
        private readonly IKuduContext _context;
        private readonly IApplicationService _applicationService;
        private readonly ICertificateSearcher _certificates;
        private readonly ICredentialProvider _credentialProvider;
        private readonly ISettingsService _settingsService;

        public ApplicationsController(
            KuduEnvironment environment,
            IKuduContext context,
            IApplicationService applicationService,
            ICertificateSearcher certificates, 
            ICredentialProvider credentialProvider,
            ISettingsService settingsService)
        {
            _environment = environment;
            _context = context;
            _applicationService = applicationService;
            _certificates = certificates;
            _credentialProvider = credentialProvider;
            _settingsService = settingsService;
        }


        [HttpGet]
        public async Task<IEnumerable<object>> Get()
        {
            return _applicationService
                .GetApplications()
                .Select(name => _applicationService.GetApplication(name))
                .Select(app => new
                {
                    name = app.Name,
                    url = app.PrimarySiteBinding.ToString()
                });
        }

        [HttpGet]
        [Route("{name}")]
        public async Task<object> Get(string name)
        {
            IApplication app = _applicationService.GetApplication(name);
            if (app == null)
                return NotFound();

            ICredentials credentials = _credentialProvider.GetCredentials();
            JObject entity = JObject.FromObject(app);
            entity["repository"] = JObject.FromObject(await app.GetRepositoryInfo(credentials));
            entity["settings"] = JObject.FromObject(await _settingsService.Get(name));
            return entity;
        }

        [HttpPost]
        public async Task<object> Post([FromBody] JObject application)
        {
            string name = (string)application["name"];
            string slug = name.GenerateSlug();

            await _applicationService.AddApplication(slug);
            return await Get(slug);
        }
    }
}
