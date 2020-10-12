using Lern_API.Helpers.Models;
using Lern_API.Models;

namespace Lern_API.DataTransferObjects.Responses
{
    public class LoginResponse : User
    {
        public string Token { get; }

        public LoginResponse(User user, string token)
        {
            this.CloneFrom(user);

            Token = token;
        }
    }
}
