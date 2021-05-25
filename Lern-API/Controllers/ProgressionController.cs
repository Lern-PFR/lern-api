using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class ProgressionController : ControllerBase
    {
        private readonly IProgressionService _progression;
        private readonly IDatabaseService<Subject, SubjectRequest> _subjects;

        public ProgressionController(IProgressionService progression, IDatabaseService<Subject, SubjectRequest> subjects)
        {
            _progression = progression;
            _subjects = subjects;
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
        /// <param name="id">The subject ID to look for</param>
        /// <returns>The current user's progression within the provided subject</returns>
        /// <response code="200">The current user's progression within the provided subject</response>
        /// <response code="204">If the current user does not have any progression information within the provided subject</response>
        /// <response code="404">If the given subject ID does not exist</response>
        [RequireAuthentication]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProgressionResponse>> GetProgression(Guid id)
        {
            var subject = await _subjects.Get(id, HttpContext.RequestAborted);

            if (subject == null)
                return NotFound();

            var progression = await _progression.Get(HttpContext.GetUser(), subject, HttpContext.RequestAborted);

            if (progression == null)
                return NoContent();

            return new ProgressionResponse(progression);
        }
    }
}
