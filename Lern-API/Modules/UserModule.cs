using System.Threading.Tasks;
using Lern_API.DataTransferObjects;
using Lern_API.Models;
using Lern_API.Repositories;
using Lern_API.Utilities;
using Nancy;

namespace Lern_API.Modules
{
    public sealed class UserModule : NancyModule
    {
        private readonly IUserRepository _users;

        public UserModule(IUserRepository users) : base("/api")
        {
            _users = users;
            
            this.GetHandlerAsync("/users", GetUsersAsync);
            this.GetHandlerAsync<UserRequest>("/user/{id}", GetUserAsync);
            this.PostHandlerAsync<CreateUserRequest>("/user/new", CreateUserAsync);
        }

        public async Task<object> GetUsersAsync()
        {
            var users = await _users.AllAsync();

            return users;
        }

        public async Task<object> GetUserAsync(UserRequest request)
        {
            var user = await _users.GetAsync(request.Id);

            if (user == null)
                return HttpStatusCode.NotFound;

            return user;
        }

        public async Task<object> CreateUserAsync(CreateUserRequest request)
        {
            var user = new User
            {
                Name = request.Name
            };

            var id = await _users.CreateAsync(user);

            if (id == 0)
            {
                return HttpStatusCode.Conflict;
            }

            return id;
        }
    }
}
