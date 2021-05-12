using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Helpers.JWT;
using Lern_API.Helpers.Models;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Moq;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class UserServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_User_By_Nickname(IMailService mailService, LoginRequest request)
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

            var service = new UserService(context, mailService);
            var result = await service.Login(request);

            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_User_By_Email(IMailService mailService, LoginRequest request)
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

            var service = new UserService(context, mailService);
            var result = await service.Login(request);

            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Null_When_Login_Does_Not_Exist(IMailService mailService, LoginRequest request, string nickname, string email)
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

            var service = new UserService(context, mailService);
            var result = await service.Login(request);

            result.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Null_When_Password_Does_Not_Match(IMailService mailService, LoginRequest request, string fakePassword)
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

            var service = new UserService(context, mailService);
            var result = await service.Login(request);

            result.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Encrypt_Password_On_Create(IMailService mailService, UserRequest request, string password)
        {
            request.Password = password;

            var context = TestSetup.SetupContext();

            var service = new UserService(context, mailService);
            var result = await service.Create(request);

            result.Should().NotBeNull();
            BCrypt.Net.BCrypt.EnhancedVerify(password, result.Password).Should().BeTrue();
        }

        [Theory]
        [AutoMoqData]
        public async Task Encrypt_Password_On_Update(IMailService mailService, Guid id, User user, UserRequest request, string password)
        {
            user.Id = id;
            request.Password = password;

            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, mailService);
            var result = await service.Update(id, request);

            result.Should().NotBeNull();
            BCrypt.Net.BCrypt.EnhancedVerify(password, result.Password).Should().BeTrue();
        }

        [Theory]
        [AutoMoqData]
        public async Task Ignore_Null_Password_On_Update(IMailService mailService, Guid id, User user, UserRequest request, string password)
        {
            user.Id = id;
            user.Password = password;
            request.Password = null;

            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, mailService);
            var result = await service.Update(id, request);

            result.Should().NotBeNull();
            result.Password.Should().Be(password);
        }

        [Theory]
        [AutoMoqData]
        public async Task Send_Forgotten_Password_Email_Using_Nickname(Mock<IMailService> mailService, User user, Uri url)
        {
            mailService.Setup(x => x.SendEmailAsync(user, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>()));

            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, mailService.Object);
            await service.ForgottenPassword(user.Nickname, url.AbsolutePath);
            
            mailService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Send_Forgotten_Password_Email_Using_Email_Address(Mock<IMailService> mailService, User user, Uri url)
        {
            mailService.Setup(x => x.SendEmailAsync(user, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>()));

            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, mailService.Object);
            await service.ForgottenPassword(user.Email, url.AbsolutePath);
            
            mailService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Always_Wait_Two_Seconds_On_Forgotten_Password(IMailService mailService, User user, Uri url)
        {
            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, mailService);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await service.ForgottenPassword(user.Email, url.AbsolutePath);

            stopwatch.Elapsed.Should().BeGreaterThan(TimeSpan.FromSeconds(2));
        }

        [Theory]
        [AutoMoqData]
        public async Task Define_Password_Using_Valid_Token(IMailService mailService, User user, string password)
        {
            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, mailService);

            var token = user.GenerateForgottenPasswordToken();

            var result = await service.DefineForgottenPassword(token, password);

            result.Should().BeTrue();
            BCrypt.Net.BCrypt.EnhancedVerify(password, context.Users.First().Password).Should().BeTrue();
        }

        [Theory]
        [AutoMoqData]
        public async Task Not_Define_Password_Using_Invalid_Token(IMailService mailService, User user, Guid invalidId, string invalidToken, string password)
        {
            var initialPassword = user.Password;

            var invalidUser = new User();
            invalidUser.CloneFrom(user);
            invalidUser.Id = invalidId;

            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, mailService);

            var token = invalidUser.GenerateForgottenPasswordToken();

            var invalidUserResult = await service.DefineForgottenPassword(token, password);
            var invalidTokenResult = await service.DefineForgottenPassword(invalidToken, password);

            invalidUserResult.Should().BeFalse();
            invalidTokenResult.Should().BeFalse();
            context.Users.First().Password.Should().Be(initialPassword);
        }
    }
}
