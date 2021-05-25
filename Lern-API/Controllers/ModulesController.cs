using System;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Helpers.JWT;
using Lern_API.Models;
using Lern_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lern_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModulesController : ControllerBase
    {
        private readonly IDatabaseService<Module, ModuleRequest> _modules;

        public ModulesController(IDatabaseService<Module, ModuleRequest> modules)
        {
            _modules = modules;
        }

        /// <summary>
        /// Returns a single module
        /// </summary>
        /// <param name="id">The ID associated to the module</param>
        /// <returns>Module associated to the provided ID</returns>
        /// <response code="200">Module associated to the provided ID</response>
        /// <response code="404">If the provided ID does not exist</response>
        [RequireAuthentication]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Module>> Get(Guid id)
        {
            var module = await _modules.Get(id, HttpContext.RequestAborted);

            if (module == null)
                return NotFound();

            return module;
        }
    }
}
