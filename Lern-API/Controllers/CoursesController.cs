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
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courses;
        private readonly IAuthorizationService _authorization;

        public CoursesController(ICourseService courses, IAuthorizationService authorization)
        {
            _courses = courses;
            _authorization = authorization;
        }

        /// <summary>
        /// Returns a single course
        /// </summary>
        /// <param name="id">The ID associated to the course</param>
        /// <returns>Course associated to the provided ID</returns>
        /// <response code="200">Course associated to the provided ID</response>
        /// <response code="404">If the provided ID does not exist</response>
        [RequireAuthentication]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Course>> Get(Guid id)
        {
            var course = await _courses.Get(id, HttpContext.RequestAborted);

            if (course == null)
                return NotFound();

            return course;
        }

        /// <summary>
        /// Create a new course
        /// </summary>
        /// <param name="course"></param>
        /// <returns>The new course</returns>
        /// <response code="200">The new course</response>
        /// <response code="409">If there has been a problem while manipulating the provided data</response>
        [RequireAuthentication]
        [HttpPost]
        public async Task<ActionResult<Course>> Create(CourseRequest course)
        {
            var result = await _courses.Create(course, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Update an existing course
        /// </summary>
        /// <param name="id">Course Id</param>
        /// <param name="course"></param>
        /// <returns>Updated course</returns>
        /// <response code="200">Updated course with the new values</response>
        /// <response code="401">If you do not have the right to update this course</response>
        /// <response code="404">If given course could not be found</response>
        [RequireAuthentication]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Course>> Update(Guid id, [CustomizeValidator(RuleSet = "Update")] CourseRequest course)
        {
            var currentUser = HttpContext.GetUser();
            var currentCourse = await _courses.Get(id, HttpContext.RequestAborted);
            
            if (currentCourse == null)
                return NotFound();

            if (!await _authorization.HasWriteAccess(currentUser, currentCourse, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _courses.Update(id, course, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Delete an existing course and all of its children
        /// </summary>
        /// <param name="id">Course Id</param>
        /// <returns>Deleted course</returns>
        /// <response code="200">Deleted course</response>
        /// <response code="401">If you do not have the right to delete this course</response>
        /// <response code="404">If given course could not be found</response>
        /// <response code="500">If an error occured while trying to delete this course</response>
        [RequireAuthentication]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Course>> Delete(Guid id)
        {
            var currentUser = HttpContext.GetUser();
            var currentCourse = await _courses.Get(id, HttpContext.RequestAborted);

            if (currentCourse == null)
                return NotFound();

            if (!await _authorization.HasAuthorship(currentUser, currentCourse, HttpContext.RequestAborted))
                return Unauthorized();

            var result = await _courses.Delete(id, HttpContext.RequestAborted);

            if (!result)
                return StatusCode(500, new ErrorResponse("An internal error occured while trying to delete this entity. Please contact an administrator if this is not intended."));

            return currentCourse;
        }
    }
}
