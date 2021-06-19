using System;
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
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessons;
        private readonly IAuthorizationService _authorization;

        public LessonsController(ILessonService lessons, IAuthorizationService authorization)
        {
            _lessons = lessons;
            _authorization = authorization;
        }

        /// <summary>
        /// Returns a single lesson
        /// </summary>
        /// <param name="id">The ID associated to the lesson</param>
        /// <returns>Lesson associated to the provided ID</returns>
        /// <response code="200">Lesson associated to the provided ID</response>
        /// <response code="404">If the provided ID does not exist</response>
        [RequireAuthentication]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Lesson>> Get(Guid id)
        {
            var lesson = await _lessons.Get(id, HttpContext.RequestAborted);

            if (lesson == null)
                return NotFound();

            return lesson;
        }

        /// <summary>
        /// Create a new lesson
        /// </summary>
        /// <param name="lesson"></param>
        /// <returns>The new lesson</returns>
        /// <response code="200">The new lesson</response>
        /// <response code="409">If there has been a problem while manipulating the provided data</response>
        [RequireAuthentication]
        [HttpPost]
        public async Task<ActionResult<Lesson>> Create(LessonRequest lesson)
        {
            var result = await _lessons.Create(lesson, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Update an existing lesson
        /// </summary>
        /// <param name="id">Lesson Id</param>
        /// <param name="lesson"></param>
        /// <returns>Updated lesson</returns>
        /// <response code="200">Updated lesson with the new values</response>
        /// <response code="401">If you do not have the right to update this lesson</response>
        /// <response code="404">If given lesson could not be found</response>
        [RequireAuthentication]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Lesson>> Update(Guid id, [CustomizeValidator(RuleSet = "Update")] LessonRequest lesson)
        {
            var currentUser = HttpContext.GetUser();
            var currentLesson = await _lessons.Get(id, HttpContext.RequestAborted);
            
            if (currentLesson == null)
                return NotFound();

            if (!await _authorization.HasWriteAccess(currentUser, currentLesson, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _lessons.Update(id, lesson, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Delete an existing lesson and all of its children
        /// </summary>
        /// <param name="id">Lesson Id</param>
        /// <returns>Deleted lesson</returns>
        /// <response code="200">Deleted lesson</response>
        /// <response code="401">If you do not have the right to delete this lesson</response>
        /// <response code="404">If given lesson could not be found</response>
        /// <response code="500">If an error occured while trying to delete this lesson</response>
        [RequireAuthentication]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Lesson>> Delete(Guid id)
        {
            var currentUser = HttpContext.GetUser();
            var currentLesson = await _lessons.Get(id, HttpContext.RequestAborted);

            if (currentLesson == null)
                return NotFound();

            if (!await _authorization.HasAuthorship(currentUser, currentLesson, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _lessons.Delete(id, HttpContext.RequestAborted);

            if (!result)
                return StatusCode(500, new ErrorResponse("An internal error occured while trying to delete this entity. Please contact an administrator if this is not intended."));

            return currentLesson;
        }
    }
}
