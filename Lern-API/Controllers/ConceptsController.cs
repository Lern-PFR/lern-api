﻿using System;
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
    public class ConceptsController : ControllerBase
    {
        private readonly IConceptService _concepts;
        private readonly IAuthorizationService _authorization;

        public ConceptsController(IConceptService concepts, IAuthorizationService authorization)
        {
            _concepts = concepts;
            _authorization = authorization;
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

        /// <summary>
        /// Create a new concept
        /// </summary>
        /// <param name="concept"></param>
        /// <returns>The new concept</returns>
        /// <response code="200">The new concept</response>
        /// <response code="409">If there has been a problem while manipulating the provided data</response>
        [RequireAuthentication]
        [HttpPost]
        public async Task<ActionResult<Concept>> Create(ConceptRequest concept)
        {
            var result = await _concepts.Create(concept, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Update an existing concept
        /// </summary>
        /// <param name="id">Concept Id</param>
        /// <param name="concept"></param>
        /// <returns>Updated concept</returns>
        /// <response code="200">Updated concept with the new values</response>
        /// <response code="401">If you do not have the right to update this concept</response>
        /// <response code="404">If given concept could not be found</response>
        [RequireAuthentication]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Concept>> Update(Guid id, [CustomizeValidator(RuleSet = "Update")] ConceptRequest concept)
        {
            var currentUser = HttpContext.GetUser();
            var currentConcept = await _concepts.Get(id, HttpContext.RequestAborted);
            
            if (currentConcept == null)
                return NotFound();

            if (!await _authorization.HasWriteAccess(currentUser, currentConcept, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _concepts.Update(id, concept, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Delete an existing concept and all of its children
        /// </summary>
        /// <param name="id">Concept Id</param>
        /// <returns>Deleted concept</returns>
        /// <response code="200">Deleted concept</response>
        /// <response code="401">If you do not have the right to delete this concept</response>
        /// <response code="404">If given concept could not be found</response>
        /// <response code="500">If an error occured while trying to delete this concept</response>
        [RequireAuthentication]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Concept>> Delete(Guid id)
        {
            var currentUser = HttpContext.GetUser();
            var currentConcept = await _concepts.Get(id, HttpContext.RequestAborted);

            if (currentConcept == null)
                return NotFound();

            if (!await _authorization.HasAuthorship(currentUser, currentConcept, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _concepts.Delete(id, HttpContext.RequestAborted);

            if (!result)
                return StatusCode(500, new ErrorResponse("An internal error occured while trying to delete this entity. Please contact an administrator if this is not intended."));

            return currentConcept;
        }
    }
}
