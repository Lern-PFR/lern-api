﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.DataTransferObjects.Responses;
using Lern_API.Helpers.JWT;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Services.Database;
using Microsoft.AspNetCore.Mvc;

namespace Lern_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _subjects;
        private readonly IAuthorizationService _authorization;

        public SubjectsController(ISubjectService subjects, IAuthorizationService authorization)
        {
            _subjects = subjects;
            _authorization = authorization;
        }

        /// <summary>
        /// Returns all subjects
        /// </summary>
        /// <returns>A list of all subjects</returns>
        [RequireAuthentication]
        [HttpGet]
        public async Task<SubjectsResponse> GetAll()
        {
            return new(
                await _subjects.GetMine(HttpContext.RequestAborted),
                await _subjects.GetActives(HttpContext.RequestAborted),
                await _subjects.GetAvailable(HttpContext.RequestAborted)
            );
        }

        /// <summary>
        /// Returns all subjects created by the current user
        /// </summary>
        /// <returns>A list of all subjects created by the current user</returns>
        [RequireAuthentication]
        [HttpGet("mine")]
        public async Task<IEnumerable<Subject>> GetMine()
        {
            return await _subjects.GetMine(HttpContext.RequestAborted);
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

            if (!await _authorization.HasWriteAccess(currentUser, currentSubject, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _subjects.Update(id, subject, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Delete an existing subject and all of its children
        /// </summary>
        /// <param name="id">Subject Id</param>
        /// <returns>Deleted subject</returns>
        /// <response code="200">Deleted subject</response>
        /// <response code="401">If you do not have the right to delete this subject</response>
        /// <response code="404">If given subject could not be found</response>
        /// <response code="500">If an error occured while trying to delete this subject</response>
        [RequireAuthentication]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Subject>> Delete(Guid id)
        {
            var currentUser = HttpContext.GetUser();
            var currentSubject = await _subjects.Get(id, HttpContext.RequestAborted);

            if (currentSubject == null)
                return NotFound();

            if (!await _authorization.HasAuthorship(currentUser, currentSubject, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _subjects.Delete(id, HttpContext.RequestAborted);

            if (!result)
                return StatusCode(500, new ErrorResponse("An internal error occured while trying to delete this entity. Please contact an administrator if this is not intended."));

            return currentSubject;
        }
    }
}
