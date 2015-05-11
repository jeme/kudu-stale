using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Kudu.Web.Models;

namespace Kudu.Web.Controllers.Api
{
    public class ApplicationsController : ApiController
    {
        private readonly IApplicationService _service;

        public ApplicationsController(IApplicationService service)
        {
            _service = service;
        }

        [HttpGet]
        public dynamic All()
        {
            return (from name in _service.GetApplications()
                                orderby name
                                select name).ToList();
        }

        [HttpPost]
        public async Task<dynamic> Add(string slug)
        {
            await _service.AddApplication(slug);

            return Task.FromResult(_service.GetApplication(slug));
        }

        [HttpDelete]
        public async Task<IHttpActionResult> Delete(string slug)
        {
            var result = await _service.DeleteApplication(slug);
            if (result)
            {
                return Ok();
            }
            return NotFound();
        }
    }
}
