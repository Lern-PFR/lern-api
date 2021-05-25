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
    public class CoursesController : ControllerBase
    {
        private readonly IDatabaseService<Course, CourseRequest> _courses;

        public CoursesController(IDatabaseService<Course, CourseRequest> courses)
        {
            _courses = courses;
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
    }
}
