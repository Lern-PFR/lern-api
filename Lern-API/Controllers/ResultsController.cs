using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Helpers.JWT;
using Lern_API.Models;
using Lern_API.Services.Database;
using Microsoft.AspNetCore.Mvc;

namespace Lern_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly IResultService _results;
        private readonly IQuestionService _questions;
        private readonly IExerciseService _exercises;

        public ResultsController(IResultService results, IQuestionService questions, IExerciseService exercises)
        {
            _results = results;
            _questions = questions;
            _exercises = exercises;
        }
        
        /// <summary>
        /// Returns a single result associated to the provided question and the current user
        /// </summary>
        /// <param name="questionId">The ID associated to the question</param>
        /// <returns>Result associated to the provided question and the current user</returns>
        /// <response code="200">Result associated to the provided question and the current user</response>
        /// <response code="204">If the current user has not completed the given question yet</response>
        /// <response code="404">If the provided question ID does not exist</response>
        [RequireAuthentication]
        [HttpGet("question/{questionId:guid}")]
        public async Task<ActionResult<Result>> GetFromQuestion(Guid questionId)
        {
            var question = await _questions.Get(questionId, HttpContext.RequestAborted);

            if (question == null)
                return NotFound();

            var result = await _results.Get(HttpContext.GetUser(), question, HttpContext.RequestAborted);

            if (result == null)
                return NoContent();

            return result;
        }

        /// <summary>
        /// Returns a list of results associated to the provided exercise and the current user
        /// </summary>
        /// <param name="exerciseId">The ID associated to the exercise</param>
        /// <returns>Results associated to the provided exercise and the current user</returns>
        /// <response code="200">Results associated to the provided exercise and the current user</response>
        /// <response code="204">If the current user has not completed any of the questions inside the provided exercise yet</response>
        /// <response code="404">If the provided exercise ID does not exist</response>
        [RequireAuthentication]
        [HttpGet("exercise/{exerciseId:guid}")]
        public async Task<ActionResult<IEnumerable<Result>>> GetFromExercise(Guid exerciseId)
        {
            var exercise = await _exercises.Get(exerciseId, HttpContext.RequestAborted);

            if (exercise == null)
                return NotFound();

            var result = await _results.GetAll(HttpContext.GetUser(), exercise, HttpContext.RequestAborted);

            if (result == null || !result.Any())
                return NoContent();

            return Ok(result);
        }

        /// <summary>
        /// Register the provided answers as the current user
        /// </summary>
        /// <param name="answers">The answers to register, each associated to the question linked to the answer</param>
        /// <response code="200">If the provided answers has successfully been registered</response>
        /// <response code="403">If the current user cannot update its progression in the subject linked to any of the answers (see simultaneous subjects restrictions)</response>
        [RequireAuthentication]
        [HttpPost]
        public async Task<IActionResult> RegisterAnswers(IEnumerable<ResultRequest> answers)
        {
            var result = await _results.RegisterAnswers(HttpContext.GetUser(), answers, HttpContext.RequestAborted);

            return result ? Ok() : Forbid();
        }
    }
}
