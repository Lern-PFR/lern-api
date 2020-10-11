﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public async Task<IEnumerable<User>> GetAllUsers() => await _users.GetAll();

        /// <summary>
        /// Returns a single user
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns>User associated to given Id</returns>
        /// <response code="200">User associated to given Id</response>
        /// <response code="404">If no user exists with given Id</response>
        [RequireAuthentication]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _users.Get(id);

            if (user == null)
                return NotFound();

            return user;
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns>User Id associated to the new user</returns>
        /// <response code="200">Id associated to the new user</response>
        /// <response code="409">If given name or email already exists in our database</response>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateUser(User user)
        {
            var id = await _users.Create(user);

            if (id == Guid.Empty)
                return Conflict();

            return id;
        }

        /// <summary>
        /// Log in to get an access token
        /// </summary>
        /// <param name="request">User credentials</param>
        /// <returns>User information and an access token</returns>
        /// <response code="200">User information and an access token</response>
        /// <response code="400">If given name or email already exists in our database</response>
        [HttpPost("/login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var response = await _users.Login(request);

            if (response == null)
                return BadRequest(new ErrorResponse("Login or password is incorrect"));

            return response;
        }
    }
}