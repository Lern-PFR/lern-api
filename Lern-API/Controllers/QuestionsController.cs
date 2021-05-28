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
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _questions;
        private readonly IAuthorizationService _authorization;

        public QuestionsController(IQuestionService questions, IAuthorizationService authorization)
        {
            _questions = questions;
            _authorization = authorization;
        }

        /// <summary>
        /// Returns a single question
        /// </summary>
        /// <param name="id">The ID associated to the question</param>
        /// <returns>Question associated to the provided ID</returns>
        /// <response code="200">Question associated to the provided ID</response>
        /// <response code="404">If the provided ID does not exist</response>
        [RequireAuthentication]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Question>> Get(Guid id)
        {
            var question = await _questions.Get(id, HttpContext.RequestAborted);

            if (question == null)
                return NotFound();

            return question;
        }

        /// <summary>
        /// Create a new question
        /// </summary>
        /// <param name="question"></param>
        /// <returns>The new question</returns>
        /// <response code="200">The new question</response>
        /// <response code="409">If there has been a problem while manipulating the provided data</response>
        [RequireAuthentication]
        [HttpPost]
        public async Task<ActionResult<Question>> Create(QuestionRequest question)
        {
            var result = await _questions.Create(question, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Update an existing question.
        /// Answers can be null if you do not want to update it
        /// </summary>
        /// <param name="id">Question Id</param>
        /// <param name="question"></param>
        /// <returns>Updated question</returns>
        /// <response code="200">Updated question with the new values</response>
        /// <response code="401">If you do not have the right to update this question</response>
        /// <response code="404">If given question could not be found</response>
        [RequireAuthentication]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Question>> Update(Guid id, [CustomizeValidator(RuleSet = "Update")] QuestionRequest question)
        {
            var currentUser = HttpContext.GetUser();
            var currentQuestion = await _questions.Get(id, HttpContext.RequestAborted);
            
            if (currentQuestion == null)
                return NotFound();

            if (!await _authorization.HasWriteAccess(currentUser, currentQuestion, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _questions.Update(id, question, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Delete an existing question and all of its children
        /// </summary>
        /// <param name="id">Question Id</param>
        /// <returns>Deleted question</returns>
        /// <response code="200">Deleted question</response>
        /// <response code="401">If you do not have the right to delete this question</response>
        /// <response code="404">If given question could not be found</response>
        /// <response code="500">If an error occured while trying to delete this question</response>
        [RequireAuthentication]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Question>> Delete(Guid id)
        {
            var currentUser = HttpContext.GetUser();
            var currentQuestion = await _questions.Get(id, HttpContext.RequestAborted);

            if (currentQuestion == null)
                return NotFound();

            if (!await _authorization.HasAuthorship(currentUser, currentQuestion, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _questions.Delete(id, HttpContext.RequestAborted);

            if (!result)
                return StatusCode(500, new ErrorResponse("An internal error occured while trying to delete this entity. Please contact an administrator if this is not intended."));

            return currentQuestion;
        }
    }
}
