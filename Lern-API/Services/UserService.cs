using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.DataTransferObjects.Responses;
using Lern_API.Helpers.JWT;
using Lern_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lern_API.Services
{
    public interface IUserService : IService<User>
    {
        Task<LoginResponse> Login(LoginRequest request, CancellationToken token = default);
    }

    public class UserService : IUserService
    {
        private readonly LernContext _context;

        public UserService(ILogger<UserService> logger, LernContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAll(CancellationToken token = default)
        {
            return await _context.Users.ToListAsync(token);
        }

        public async Task<User> Get(Guid id, CancellationToken token = default)
        {
            return await _context.Users.FindAsync(new object[] { id }, token);
        }

        public async Task<User> Create(User entity, CancellationToken token = default)
        {
            entity.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(entity.Password);

            var entityEntry = await _context.Users.AddAsync(entity, token);
            await _context.SaveChangesAsync(token);

            return entityEntry.Entity;
        }

        public async Task<User> Update(User entity, IEnumerable<string> columns, CancellationToken token = default)
        {
            if (entity.Password != null)
                entity.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(entity.Password);

            var user = await Get(entity.Id, token);
            var props = user.GetType().GetProperties(BindingFlags.Public);

            foreach (var key in columns)
            {
                var prop = props.First(x => x.Name == key);
                prop.SetValue(user, prop.GetValue(entity));
            }

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync(token);

            return user;
        }

        public async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            var user = await _context.Users.FindAsync(new object[] { id }, token);

            if (user == null)
                return false;

            _context.Remove(user);
            return true;
        }

        public async Task<bool> Exists(Guid id, CancellationToken token = default)
        {
            return await _context.Users.AnyAsync(x => x.Id == id, token);
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
