using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class RootController : ControllerBase
    {
        private IUrlHelper urlHelper;
        public RootController(IUrlHelper _urlHelper)
        {
            urlHelper = _urlHelper;
        }
        //[HttpGet(Name = "GetRoot")]
        //public IActionResult GetRoot([FromHeader(Name="Accept")] string mediaType)
        //{
        //    if (mediaType.ToLowerInvariant() == "application/vnd.marvin.hateoas+json")
        //    {
        //        var link = new List<LinkDto>();
        //    }
        //}
      
    }
}