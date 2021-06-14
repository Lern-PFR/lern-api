using System;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.DataTransferObjects.Responses;
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
        private readonly IAuthorizationService _authorization;

        public ModulesController(IDatabaseService<Module, ModuleRequest> modules, IAuthorizationService authorization)
        {
            _modules = modules;
            _authorization = authorization;
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

        /// <summary>
        /// Create a new module
        /// </summary>
        /// <param name="module"></param>
        /// <returns>The new module</returns>
        /// <response code="200">The new module</response>
        /// <response code="409">If there has been a problem while manipulating the provided data</response>
        [RequireAuthentication]
        [HttpPost]
        public async Task<ActionResult<Module>> Create(ModuleRequest module)
        {
            var result = await _modules.Create(module, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Update an existing module
        /// </summary>
        /// <param name="id">Module Id</param>
        /// <param name="module"></param>
        /// <returns>Updated module</returns>
        /// <response code="200">Updated module with the new values</response>
        /// <response code="401">If you do not have the right to update this module</response>
        /// <response code="404">If given module could not be found</response>
        [RequireAuthentication]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Module>> Update(Guid id, [CustomizeValidator(RuleSet = "Update")] ModuleRequest module)
        {
            var currentUser = HttpContext.GetUser();
            var currentModule = await _modules.Get(id, HttpContext.RequestAborted);
            
            if (currentModule == null)
                return NotFound();

            if (!await _authorization.HasWriteAccess(currentUser, currentModule, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _modules.Update(id, module, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Delete an existing module and all of its children
        /// </summary>
        /// <param name="id">Module Id</param>
        /// <returns>Deleted module</returns>
        /// <response code="200">Deleted module</response>
        /// <response code="401">If you do not have the right to delete this module</response>
        /// <response code="404">If given module could not be found</response>
        /// <response code="500">If an error occured while trying to delete this module</response>
        [RequireAuthentication]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Module>> Delete(Guid id)
        {
            var currentUser = HttpContext.GetUser();
            var currentModule = await _modules.Get(id, HttpContext.RequestAborted);

            if (currentModule == null)
                return NotFound();

            if (!await _authorization.HasAuthorship(currentUser, currentModule, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _modules.Delete(id, HttpContext.RequestAborted);

            if (!result)
                return StatusCode(500, new ErrorResponse("An internal error occured while trying to delete this entity. Please contact an administrator if this is not intended."));

            return currentModule;
        }
    }
}
