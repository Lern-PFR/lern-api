using System;
using System.Collections.Generic;
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
    public class SubjectsController : ControllerBase
    {
        private readonly IDatabaseService<Subject, SubjectRequest> _subjects;

        public SubjectsController(IDatabaseService<Subject, SubjectRequest> subjects)
        {
            _subjects = subjects;
        }

        /// <summary>
        /// Returns all subjects
        /// </summary>
        /// <returns>A list of all subjects</returns>
        [RequireAuthentication]
        [HttpGet]
        public async Task<IEnumerable<Subject>> GetAll()
        {
            return await _subjects.GetAll(HttpContext.RequestAborted);
        }

        /// <summary>
        /// Returns a single subject
        /// </summary>
        /// <param name="id">The ID associated to the subject</param>
        /// <returns>Subject associated to the provided ID</returns>
        /// <response code="200">Subject associated to the provided ID</response>
        /// <response code="404">If the provided ID does not exist</response>
        [RequireAuthentication]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Subject>> Get(Guid id)
        {
            var subject = await _subjects.Get(id, HttpContext.RequestAborted);

            if (subject == null)
                return NotFound();

            return subject;
        }
    }
}
