using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.DataTransferObjects.Responses;
using Lern_API.Helpers;
using Lern_API.Helpers.JWT;
using Lern_API.Helpers.Models;
using Lern_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services.Database
{
    public interface IUserService : IDatabaseService<User, UserRequest>
    {
        Task<LoginResponse> Login(LoginRequest request, CancellationToken token = default);
        Task ForgottenPassword(string login, string appBaseUrl, CancellationToken token = default);
        Task<bool> DefineForgottenPassword(string userToken, string password, CancellationToken token = default);
    }

    public class UserService : DatabaseService<User, UserRequest>, IUserService
    {
        private readonly IMailService _mails;

        public UserService(LernContext context, IHttpContextAccessor httpContextAccessor, IMailService mails) : base(context, httpContextAccessor)
        {
            _mails = mails;
        }

        protected override IQueryable<User> WithDefaultIncludes(DbSet<User> set)
        {
            return base.WithDefaultIncludes(set)
                .Include(user => user.Manager);
        }

        public override async Task<User> Create(UserRequest entity, CancellationToken token = default)
        {
            entity.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(entity.Password);
            entity.Email = entity.Email.ToLowerInvariant();

            var user = await base.Create(entity, token);

            if (user == null)
                return null;
            
            await _mails.SendEmailAsync(user, "Création de votre compte Lern.", "Welcome", new
            {
                AssetsUrl = Url.Combine(Configuration.Get<string>("ApiHost"), "assets"),
                HomePageUrl = Configuration.Get<string>("FrontendHost"),
                Username = user.Nickname
            }, token);

            return user;
        }

        public override async Task<User> Update(Guid id, UserRequest entity, CancellationToken token = default)
        {
            if (entity.Password != null)
                entity.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(entity.Password);
            else
            {
                var storedUser = await Get(id, token);
                entity.Password = storedUser.Password;
            }

            entity.Email = entity.Email.ToLowerInvariant();

            return await base.Update(id, entity, token);
        }

        public async Task<LoginResponse> Login(LoginRequest request, CancellationToken token = default)
        {
            var user = await WithDefaultIncludes(DbSet).FirstOrDefaultAsync(x => x.Nickname == request.Login || x.Email == request.Login.ToLowerInvariant(), token);

            if (user == null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.Password))
                return null;

            return new LoginResponse(user, user.GenerateToken());
        }

        public async Task ForgottenPassword(string login, string appBaseUrl, CancellationToken token = default)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var user = await WithDefaultIncludes(DbSet).FirstOrDefaultAsync(x => x.Nickname == login || x.Email == login.ToLowerInvariant(), token);

            if (user != null)
            {
                await _mails.SendEmailAsync(user, "Récupération du compte", "RecoverAccount", new
                {
                    AssetsUrl = Url.Combine(Configuration.Get<string>("ApiHost"), "assets"),
                    HomePageUrl = Configuration.Get<string>("FrontendHost"),
                    Username = user.Nickname,
                    GeneratedLink = Url.Combine(Configuration.Get<string>("FrontendHost"), user.GenerateForgottenPasswordToken())
                }, token);
            }
            
            if (stopwatch.ElapsedMilliseconds < 2000)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(2005 - stopwatch.ElapsedMilliseconds), token);
            }
        }

        public async Task<bool> DefineForgottenPassword(string userToken, string password, CancellationToken token = default)
        {
            var userId = JwtExtensions.GetUserIdFromToken(userToken);

            if (!userId.HasValue)
                return false;

            var user = await Get(userId.Value, token);

            if (user == null)
                return false;

            var userRequest = new UserRequest();
            userRequest.CloneFrom(user);
            userRequest.Password = password;

            var result = await Update(userId.Value, userRequest, token);

            return result != null;
        }
    }
}
