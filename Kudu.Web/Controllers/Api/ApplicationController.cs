using System;
using System.Web.Http;
using Kudu.SiteManagement;
using Kudu.Web.Models;
using Newtonsoft.Json.Linq;

namespace Kudu.Web.Controllers.Api
{
    public class ApplicationController : ApiController
    {
        private readonly IApplicationService _service;

        public ApplicationController(IApplicationService service)
        {
            _service = service;
        }

        [HttpGet]
        public dynamic Get(string slug)
        {
            IApplication application = _service.GetApplication(slug);
            if (application != null)
            {
                return application;
            }
            return NotFound();
        }

        [HttpPost]
        public dynamic AddBinding(string slug, [FromBody] JObject json)
        {
            KuduBinding binding = json.ToObject<KuduBinding>();
            if (_service.AddSiteBinding(slug, binding))
            {
                return binding;
            }
            return base.BadRequest("Could not add binding to site.");
        }

        [HttpPost]
        public dynamic RemoveBinding(string slug, [FromBody] JObject json)
        {
            KuduBinding binding = json.ToObject<KuduBinding>();
            switch (binding.SiteType)
            {
                case SiteType.Live:
                    if (_service.RemoveLiveSiteBinding(slug, binding))
                    {
                        return true;
                    }
                    break;
                case SiteType.Service:
                    if (_service.RemoveServiceSiteBinding(slug, binding))
                    {
                        return true;
                    }
                    break;
            }
            return NotFound();
        }
    }
}