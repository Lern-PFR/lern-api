using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.Controllers;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.DataTransferObjects.Responses;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Lern_API.Tests.Controllers
{
    public class UsersControllerShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Return_All_Users(Mock<IUserService> service, List<User> users)
        {
            service.Setup(x => x.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(users);

            var controller = TestSetup.SetupController<UsersController>(service.Object);

            var result = await controller.GetAllUsers();

            Assert.NotNull(result);
            Assert.Equal(users.Count, result.Count());
            Assert.True(users.All(x => result.Any(y => y == x)));
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_User_Or_404(Mock<IUserService> service, User user, Guid goodGuid, Guid badGuid)
        {
            service.Setup(x => x.Get(goodGuid, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            service.Setup(x => x.Get(badGuid, It.IsAny<CancellationToken>())).ReturnsAsync((User) null);

            var controller = TestSetup.SetupController<UsersController>(service.Object);

            var result = await controller.GetUser(goodGuid);
            var invalidResult = await controller.GetUser(badGuid);

            Assert.NotNull(result.Value);
            Assert.Equal(result.Value, user);

            Assert.Null(invalidResult.Value);
            Assert.IsType<NotFoundResult>(invalidResult.Result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Create_User_Or_409(Mock<IUserService> service, User user)
        {
            service.Setup(x => x.Create(user, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            service.Setup(x => x.Create(null, It.IsAny<CancellationToken>())).ReturnsAsync((User) null);

            var controller = TestSetup.SetupController<UsersController>(service.Object);

            var goodUser = await controller.CreateUser(user);
            var invalidUser = await controller.CreateUser(null);

            Assert.NotNull(goodUser.Value);
            Assert.Null(invalidUser.Value);
            Assert.IsType<ConflictResult>(invalidUser.Result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_User_Or_409(Mock<IUserService> service, User validUser, User invalidUser)
        {
            service.Setup(x => x.Update(validUser, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(validUser);
            service.Setup(x => x.Update(invalidUser, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync((User) null);

            var controller = TestSetup.SetupController<UsersController>(service.Object);

            var goodResult = await controller.UpdateUser(Guid.NewGuid(), validUser);
            var invalidResult = await controller.UpdateUser(Guid.NewGuid(), invalidUser);

            Assert.Equal(validUser, goodResult.Value);

            Assert.Null(invalidResult.Value);
            Assert.IsType<NotFoundResult>(invalidResult.Result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Login_Or_400(Mock<IUserService> service, LoginRequest validRequest, LoginRequest invalidRequest, LoginResponse validResponse)
        {
            service.Setup(x => x.Login(validRequest, It.IsAny<CancellationToken>())).ReturnsAsync(validResponse);
            service.Setup(x => x.Login(invalidRequest, It.IsAny<CancellationToken>())).ReturnsAsync((LoginResponse) null);

            var controller = TestSetup.SetupController<UsersController>(service.Object);

            var goodResult = await controller.Login(validRequest);
            var invalidResult = await controller.Login(invalidRequest);

            Assert.Equal(validResponse, goodResult.Value);

            Assert.Null(invalidResult.Value);
            Assert.IsType<BadRequestObjectResult>(invalidResult.Result);
        }
    }
}
