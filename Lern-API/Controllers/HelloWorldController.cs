using Microsoft.AspNetCore.Mvc;

namespace Lern_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelloWorldController : ControllerBase
    {
        /// <summary>
        /// Returns "Hello, world!", can be used to test connectivity
        /// </summary>
        /// <returns>Hello, world!</returns>
        [HttpGet]
        public string Index()
        {
            return "Hello, world!";
        }
    }
}
