using System;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.DataTransferObjects.Responses;
using Lern_API.Helpers.JWT;
using Lern_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services
{
    public interface IUserService : IService<User, UserRequest>
    {
        Task<LoginResponse> Login(LoginRequest request, CancellationToken token = default);
    }

    public class UserService : Service<User, UserRequest>, IUserService
    {
        private readonly LernContext _context;

        public UserService(LernContext context) : base(context)
        {
            _context = context;
        }
        
        public new async Task<User> Create(UserRequest entity, CancellationToken token = default)
        {
            entity.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(entity.Password);

            return await base.Create(entity, token);
        }

        public new async Task<User> Update(Guid id, UserRequest entity, CancellationToken token = default)
        {
            if (entity.Password != null)
                entity.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(entity.Password);

            return await base.Update(id, entity, token);
        }

        public async Task<LoginResponse> Login(LoginRequest request, CancellationToken token = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Nickname == request.Login || x.Email == request.Login, token);

            if (user == null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.Password))
                return null;

            return new LoginResponse(user, user.GenerateToken());
        }
    }
}
