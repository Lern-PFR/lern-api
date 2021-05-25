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
    public class ExercisesController : ControllerBase
    {
        private readonly IDatabaseService<Exercise, ExerciseRequest> _exercises;

        public ExercisesController(IDatabaseService<Exercise, ExerciseRequest> exercises)
        {
            _exercises = exercises;
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
    }
}
