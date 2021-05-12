using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.DataTransferObjects.Responses;
using Lern_API.Helpers.Context;
using Lern_API.Helpers.JWT;
using Lern_API.Models;
using Lern_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lern_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _users;

        public UsersController(IUserService users)
        {
            _users = users;
        }

        /// <summary>
        /// Returns all registered users
        /// </summary>
        /// <returns>A list of all registered users</returns>
        [RequireAuthentication]
        [HttpGet]
        public async Task<IEnumerable<UserResponse>> GetAllUsers()
        {
            var users = await _users.GetAll(HttpContext.RequestAborted);

            return users.Select(user => new UserResponse(user));
        }

        /// <summary>
        /// Returns a single user
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns>User associated to given Id</returns>
        /// <response code="200">User associated to given Id</response>
        /// <response code="404">If given user could not be found</response>
        [RequireAuthentication]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserResponse>> GetUser(Guid id)
        {
            var user = await _users.Get(id, HttpContext.RequestAborted);

            if (user == null)
                return NotFound();

            return new UserResponse(user);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns>User Id associated to the new user</returns>
        /// <response code="200">Id associated to the new user</response>
        /// <response code="409">If given name or email already exists</response>
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(UserRequest user)
        {
            var result = await _users.Create(user, HttpContext.RequestAborted);

            if (result == null)
                return Conflict();

            return result;
        }

        /// <summary>
        /// Update an existing user.
        /// Password can be null if you do not want to update it
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="user"></param>
        /// <returns>Updated user</returns>
        /// <response code="200">Updated user with the new values</response>
        /// <response code="401">If you do not have the right to update this user</response>
        /// <response code="404">If given user could not be found</response>
        [RequireAuthentication]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<User>> UpdateUser(Guid id, [CustomizeValidator(RuleSet = "Update")] UserRequest user)
        {
            var currentUser = HttpContext.GetUser();

            if (currentUser.Id != id && !currentUser.Admin)
                return Unauthorized();

            var exists = await _users.Exists(id, HttpContext.RequestAborted);

            if (!exists)
                return NotFound();

            var newUser = await _users.Update(id, user, HttpContext.RequestAborted);

            if (newUser == null)
                return Conflict();

            return newUser;
        }

        /// <summary>
        /// Log in to get an access token
        /// </summary>
        /// <param name="request">User credentials</param>
        /// <returns>User information and an access token</returns>
        /// <response code="200">User information and an access token</response>
        /// <response code="400">If given name or email already exists in our database</response>
        [HttpPost("/api/Login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var response = await _users.Login(request, HttpContext.RequestAborted);

            if (response == null)
                return BadRequest(new ErrorResponse("Login or password is incorrect"));

            return response;
        }

        /// <summary>
        /// Send an email with a password definition link to the user associated to the given nickname or email address.
        /// </summary>
        /// <remarks>
        /// This route will always take more than 2 seconds to complete, to avoid timing attacks.
        /// </remarks>
        /// <param name="login">The nickname or the email address of the user</param>
        /// <response code="200">This route always sends an empty OK response</response>
        [HttpGet("/api/ForgottenPassword")]
        public async Task<IActionResult> ForgottenPassword([FromQuery] string login)
        {
            await _users.ForgottenPassword(login, HttpContext.GetBaseUrl(), HttpContext.RequestAborted);

            return Ok();
        }

        /// <summary>
        /// Define a new password for the user associated to the given token.
        /// </summary>
        /// <remarks>
        /// To get a valid token, please use the <c>/api/ForgottenPassword</c> route
        /// </remarks>
        /// <param name="request">The token sent by email along with the new password</param>
        /// <returns></returns>
        [HttpPost("/api/ForgottenPassword")]
        public async Task<IActionResult> ForgottenPasswordDefinition(ForgottenPasswordDefinitionRequest request)
        {
            var result = await _users.DefineForgottenPassword(request.Token, request.Password, HttpContext.RequestAborted);

            if (!result)
                return BadRequest();

            return Ok();
        }
        
        /// <summary>
        /// Returns the currently logged in user
        /// </summary>
        /// <returns>User information</returns>
        /// <response code="200">User information</response>
        /// <response code="401">If the current session token expired or is invalid</response>
        [RequireAuthentication]
        [HttpGet("/api/Whoami")]
        public ActionResult<User> Whoami()
        {
            return HttpContext.GetUser();
        }
    }
}
