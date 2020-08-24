using Lern_API.Repositories;
using Nancy;

namespace Lern_API.Modules
{
    public class LoginModule : NancyModule
    {
        private readonly IUserRepository _users;

        public LoginModule(IUserRepository users) : base("/api")
        {
            _users = users;
        }
    }
}
