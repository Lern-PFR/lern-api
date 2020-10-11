using System;
using Lern_API.Models;

namespace Lern_API.DataTransferObjects.Responses
{
    public class LoginResponse
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Email { get; }
        public string Token { get; }

        public LoginResponse(User user, string token)
        {
            Id = user.Id;
            Name = user.Name;
            Email = user.Email;
            Token = token;
        }
    }
}
