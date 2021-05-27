using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
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

        /// <summary>
        /// Create a new subject
        /// </summary>
        /// <param name="subject"></param>
        /// <returns>The new subject</returns>
        /// <response code="200">The new subject</response>
        /// <response code="409">If there has been a problem while manipulating the provided data</response>
        [RequireAuthentication]
        [HttpPost]
        public async Task<ActionResult<Subject>> Create(SubjectRequest subject)
        {
            var result = await _subjects.Create(subject, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Update an existing subject
        /// </summary>
        /// <param name="id">Subject Id</param>
        /// <param name="subject"></param>
        /// <returns>Updated subject</returns>
        /// <response code="200">Updated subject with the new values</response>
        /// <response code="401">If you do not have the right to update this subject</response>
        /// <response code="404">If given subject could not be found</response>
        [RequireAuthentication]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Subject>> Update(Guid id, [CustomizeValidator(RuleSet = "Update")] SubjectRequest subject)
        {
            var currentUser = HttpContext.GetUser();
            var currentSubject = await _subjects.Get(id, HttpContext.RequestAborted);
            
            if (currentSubject == null)
                return NotFound();

            if (currentSubject.AuthorId != currentUser.Id && !currentUser.Admin)
                return Unauthorized();

            var result = await _subjects.Update(id, subject, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }
    }
}
