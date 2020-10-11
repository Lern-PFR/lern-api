using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace Lern_API.Controllers
{
    [Route("{**url}", Order = int.MaxValue)]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        /// <summary>
        /// Serves a SPA (Single Page Application), it is NOT a part of the official API and is subject to changes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CatchAll()
        {
            return File(
                new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Content", "index.html"), FileMode.Open),
                "text/html; charset=utf-8");
        }
    }
}
