using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kudu.Client.Infrastructure;
using Kudu.SiteManagement.Certificates;
using Kudu.SiteManagement.Context;
using Kudu.Web.Infrastructure;
using Kudu.Web.Models;

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
        public async Task<IEnumerable<string>> Get()
        {
            return _applicationService.GetApplications();
        }
    }


    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
