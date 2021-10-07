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
using Kudu.Web.Infrastructure;
using Kudu.Web.Models;
using Newtonsoft.Json.Linq;

namespace Kudu.Web5.Controllers
{
    [ApiController]
    [Route("api/v1/applications")]
    public class ApplicationsController : ControllerBase
    {
        private readonly KuduEnvironment _environment;
        private readonly IKuduContext _context;
        private readonly IApplicationService _applicationService;
        private readonly ICertificateSearcher _certificates;
        private readonly ICredentialProvider _credentialProvider;

        public ApplicationsController(
            KuduEnvironment environment,
            IKuduContext context,
            IApplicationService applicationService,
            ICertificateSearcher certificates, 
            ICredentialProvider credentialProvider)
        {
            _environment = environment;
            _context = context;
            _applicationService = applicationService;
            _certificates = certificates;
            _credentialProvider = credentialProvider;
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


    //[ApiController]
    //[Route("[controller]")]
    //public class WeatherForecastController : ControllerBase
    //{
    //    private static readonly string[] Summaries = new[]
    //    {
    //        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    //    };

    //    private readonly ILogger<WeatherForecastController> _logger;

    //    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    //    {
    //        _logger = logger;
    //    }

    //    [HttpGet]
    //    public IEnumerable<WeatherForecast> Get()
    //    {
    //        var rng = new Random();
    //        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
    //        {
    //            Date = DateTime.Now.AddDays(index),
    //            TemperatureC = rng.Next(-20, 55),
    //            Summary = Summaries[rng.Next(Summaries.Length)]
    //        })
    //        .ToArray();
    //    }
    //}
}
