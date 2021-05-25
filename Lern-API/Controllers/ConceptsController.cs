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
    public class ConceptsController : ControllerBase
    {
        private readonly IDatabaseService<Concept, ConceptRequest> _concepts;

        public ConceptsController(IDatabaseService<Concept, ConceptRequest> concepts)
        {
            _concepts = concepts;
        }

        /// <summary>
        /// Returns a single concept
        /// </summary>
        /// <param name="id">The ID associated to the concept</param>
        /// <returns>Concept associated to the provided ID</returns>
        /// <response code="200">Concept associated to the provided ID</response>
        /// <response code="404">If the provided ID does not exist</response>
        [RequireAuthentication]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Concept>> Get(Guid id)
        {
            var concept = await _concepts.Get(id, HttpContext.RequestAborted);

            if (concept == null)
                return NotFound();

            return concept;
        }
    }
}
