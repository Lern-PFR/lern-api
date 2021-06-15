using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.DataTransferObjects.Responses;
using Lern_API.Helpers.JWT;
using Lern_API.Services.Database;
using Microsoft.AspNetCore.Mvc;

namespace Lern_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgressionController : ControllerBase
    {
        private readonly IProgressionService _progression;
        private readonly ISubjectService _subjects;
        private readonly IConceptService _concepts;

        public ProgressionController(IProgressionService progression, ISubjectService subjects, IConceptService concepts)
        {
            _progression = progression;
            _subjects = subjects;
            _concepts = concepts;
        }

        /// <summary>
        /// Returns all of the current user's progressions
        /// </summary>
        /// <returns>A list of all of the current user's progressions</returns>
        [RequireAuthentication]
        [HttpGet]
        public async Task<IEnumerable<ProgressionResponse>> GetProgressions()
        {
            var progressions = await _progression.GetAll(HttpContext.GetUser(), HttpContext.RequestAborted);

            return progressions.Select(x => new ProgressionResponse(x));
        }

        /// <summary>
        /// Returns the current user's progression within the provided subject
        /// </summary>
        /// <param name="subjectId">The subject ID to look for</param>
        /// <returns>The current user's progression within the provided subject</returns>
        /// <response code="200">The current user's progression within the provided subject</response>
        /// <response code="204">If the current user does not have any progression information within the provided subject</response>
        /// <response code="404">If the given subject ID does not exist</response>
        [RequireAuthentication]
        [HttpGet("{subjectId:guid}")]
        public async Task<ActionResult<ProgressionResponse>> GetProgression(Guid subjectId)
        {
            var subject = await _subjects.Get(subjectId, HttpContext.RequestAborted);

            if (subject == null)
                return NotFound();

            var progression = await _progression.Get(HttpContext.GetUser(), subject, HttpContext.RequestAborted);

            if (progression == null)
                return NoContent();

            return new ProgressionResponse(progression);
        }

        /// <summary>
        /// Updates the current user's progression in the provided subject
        /// </summary>
        /// <param name="request">The subject to update to the provided concept</param>
        /// <response code="200">If the progression has successfully been updated</response>
        /// <response code="403">If the current user cannot update its progression in the provided subject (see simultaneous subjects restrictions)</response>
        [RequireAuthentication]
        [HttpPut]
        public async Task<IActionResult> UpdateProgression(ProgressionRequest request)
        {
            var subject = await _subjects.Get(request.SubjectId, HttpContext.RequestAborted);
            var concept = await _concepts.Get(request.ConceptId, HttpContext.RequestAborted);
            
            var result = await _progression.Update(HttpContext.GetUser(), subject, concept, HttpContext.RequestAborted);

            return result ? Ok() : Forbid();
        }
    }
}
