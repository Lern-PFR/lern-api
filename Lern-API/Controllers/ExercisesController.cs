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
    public class ExercisesController : ControllerBase
    {
        private readonly IDatabaseService<Exercise, ExerciseRequest> _exercises;
        private readonly IAuthorizationService _authorization;

        public ExercisesController(IDatabaseService<Exercise, ExerciseRequest> exercises, IAuthorizationService authorization)
        {
            _exercises = exercises;
            _authorization = authorization;
        }

        /// <summary>
        /// Returns a single exercise
        /// </summary>
        /// <param name="id">The ID associated to the exercise</param>
        /// <returns>Exercise associated to the provided ID</returns>
        /// <response code="200">Exercise associated to the provided ID</response>
        /// <response code="404">If the provided ID does not exist</response>
        [RequireAuthentication]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Exercise>> Get(Guid id)
        {
            var exercise = await _exercises.Get(id, HttpContext.RequestAborted);

            if (exercise == null)
                return NotFound();

            return exercise;
        }

        /// <summary>
        /// Create a new exercise
        /// </summary>
        /// <param name="exercise"></param>
        /// <returns>The new exercise</returns>
        /// <response code="200">The new exercise</response>
        /// <response code="409">If there has been a problem while manipulating the provided data</response>
        [RequireAuthentication]
        [HttpPost]
        public async Task<ActionResult<Exercise>> Create(ExerciseRequest exercise)
        {
            var result = await _exercises.Create(exercise, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Update an existing exercise
        /// </summary>
        /// <param name="id">Exercise Id</param>
        /// <param name="exercise"></param>
        /// <returns>Updated exercise</returns>
        /// <response code="200">Updated exercise with the new values</response>
        /// <response code="401">If you do not have the right to update this exercise</response>
        /// <response code="404">If given exercise could not be found</response>
        [RequireAuthentication]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Exercise>> Update(Guid id, [CustomizeValidator(RuleSet = "Update")] ExerciseRequest exercise)
        {
            var currentUser = HttpContext.GetUser();
            var currentExercise = await _exercises.Get(id, HttpContext.RequestAborted);
            
            if (currentExercise == null)
                return NotFound();

            if (!await _authorization.HasWriteAccess(currentUser, currentExercise, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _exercises.Update(id, exercise, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Delete an existing exercise and all of its children
        /// </summary>
        /// <param name="id">Exercise Id</param>
        /// <returns>Deleted exercise</returns>
        /// <response code="200">Deleted exercise</response>
        /// <response code="401">If you do not have the right to delete this exercise</response>
        /// <response code="404">If given exercise could not be found</response>
        /// <response code="500">If an error occured while trying to delete this exercise</response>
        [RequireAuthentication]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Exercise>> Delete(Guid id)
        {
            var currentUser = HttpContext.GetUser();
            var currentExercise = await _exercises.Get(id, HttpContext.RequestAborted);

            if (currentExercise == null)
                return NotFound();

            if (!await _authorization.HasAuthorship(currentUser, currentExercise, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _exercises.Delete(id, HttpContext.RequestAborted);

            if (!result)
                return StatusCode(500, new ErrorResponse("An internal error occured while trying to delete this entity. Please contact an administrator if this is not intended."));

            return currentExercise;
        }
    }
}
