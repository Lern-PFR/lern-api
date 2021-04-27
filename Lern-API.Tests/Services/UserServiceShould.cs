using System;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class UserServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_User_By_Nickname(LoginRequest request)
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

            var service = new UserService(context);
            var result = await service.Login(request);

            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_User_By_Email(LoginRequest request)
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

            var service = new UserService(context);
            var result = await service.Login(request);

            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Null_When_Login_Does_Not_Exist(LoginRequest request, string nickname, string email)
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

            var service = new UserService(context);
            var result = await service.Login(request);

            result.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Null_When_Password_Does_Not_Match(LoginRequest request, string fakePassword)
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

            var service = new UserService(context);
            var result = await service.Login(request);

            result.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Encrypt_Password_On_Create(UserRequest request, string password)
        {
            request.Password = password;

            var context = TestSetup.SetupContext();

            var service = new UserService(context);
            var result = await service.Create(request);

            result.Should().NotBeNull();
            BCrypt.Net.BCrypt.EnhancedVerify(password, result.Password).Should().BeTrue();
        }

        [Theory]
        [AutoMoqData]
        public async Task Encrypt_Password_On_Update(Guid id, User user, UserRequest request, string password)
        {
            user.Id = id;
            request.Password = password;

            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);
            var result = await service.Update(id, request);

            result.Should().NotBeNull();
            BCrypt.Net.BCrypt.EnhancedVerify(password, result.Password).Should().BeTrue();
        }

        [Theory]
        [AutoMoqData]
        public async Task Ignore_Null_Password_On_Update(Guid id, User user, UserRequest request, string password)
        {
            user.Id = id;
            user.Password = password;
            request.Password = null;

            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);
            var result = await service.Update(id, request);

            result.Should().NotBeNull();
            result.Password.Should().Be(password);
        }
    }
}
