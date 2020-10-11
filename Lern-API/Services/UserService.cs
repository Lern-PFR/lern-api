using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.DataTransferObjects.Responses;
using Lern_API.Helpers.JWT;
using Lern_API.Models;
using Lern_API.Repositories;
using Microsoft.Extensions.Logging;

namespace Lern_API.Services
{
    public interface IUserService : IService<User, IUserRepository>
    {
        Task<LoginResponse> Login(LoginRequest request);
    }

    public class UserService : Service<User, IUserRepository>, IUserService
    {
        public UserService(ILogger<UserService> logger, IUserRepository repository) : base(logger, repository)
        {
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var user = await Repository.GetByLogin(request.Login);

            if (user == null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.Password))
                return null;

            return new LoginResponse(user, user.GenerateToken());
        }
    }
}
