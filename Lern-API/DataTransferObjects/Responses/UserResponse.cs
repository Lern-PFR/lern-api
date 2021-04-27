using System;
using Lern_API.Helpers.Models;
using Lern_API.Models;

namespace Lern_API.DataTransferObjects.Responses
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Nickname { get; set; }
        public bool Active { get; set; } = true;
        public bool Admin { get; set; } = false;
        public bool VerifiedCreator { get; set; } = false;

        public UserResponse(User user)
        {
            this.CloneFrom(user);
        }
    }
}
