using System;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class UserServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_User_By_Nickname(ILogger<UserService> logger, LoginRequest request)
        {
            var context = TestSetup.SetupContext();
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                Nickname = request.Login,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password)
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(logger, context);
            var result = await service.Login(request);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_User_By_Email(ILogger<UserService> logger, LoginRequest request)
        {
            var context = TestSetup.SetupContext();
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Login,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password)
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(logger, context);
            var result = await service.Login(request);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Null_When_Login_Does_Not_Exist(ILogger<UserService> logger, LoginRequest request, string nickname, string email)
        {
            var context = TestSetup.SetupContext();
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                Nickname = nickname,
                Email = email,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password)
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(logger, context);
            var result = await service.Login(request);

            Assert.Null(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Null_When_Password_Do_Not_Match(ILogger<UserService> logger, LoginRequest request, string fakePassword)
        {
            var context = TestSetup.SetupContext();
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                Nickname = request.Login,
                Email = request.Login,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(fakePassword)
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(logger, context);
            var result = await service.Login(request);

            Assert.Null(result);
        }
    }
}
