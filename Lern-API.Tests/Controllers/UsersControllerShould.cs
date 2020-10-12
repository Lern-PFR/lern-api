using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lern_API.Controllers;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.DataTransferObjects.Responses;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Tests.Attributes;
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
            service.Setup(x => x.GetAll()).ReturnsAsync(users);

            var controller = new UsersController(service.Object);

            var result = await controller.GetAllUsers();

            Assert.NotNull(result);
            Assert.Equal(users.Count, result.Count());
            Assert.True(users.All(x => result.Any(y => y == x)));
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_User_Or_404(Mock<IUserService> service, List<User> users)
        {
            service.Setup(x => x.Get(It.IsAny<Guid>())).Returns<Guid>(async id => users.SingleOrDefault(x => x.Id == id));

            var controller = new UsersController(service.Object);

            var user1 = await controller.GetUser(users.First().Id);
            var invalidUser = await controller.GetUser(Guid.NewGuid());

            Assert.NotNull(user1.Value);
            Assert.Equal(users.First(), user1.Value);

            Assert.Null(invalidUser.Value);
            Assert.IsType<NotFoundResult>(invalidUser.Result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Create_User_Or_409(Mock<IUserService> service, User user)
        {
            service.Setup(x => x.Create(user)).Returns<User>(async user => Guid.NewGuid());
            service.Setup(x => x.Create(null)).Returns<User>(async user => Guid.Empty);

            var controller = new UsersController(service.Object);

            var goodId = await controller.CreateUser(user);
            var invalidId = await controller.CreateUser(null);

            Assert.NotEqual(Guid.Empty, goodId.Value);

            Assert.Equal(default, invalidId.Value);
            Assert.IsType<ConflictResult>(invalidId.Result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_User_Or_409(Mock<IUserService> service, User validUser, User invalidUser)
        {
            service.Setup(x => x.Update(validUser, It.IsAny<IEnumerable<string>>())).Returns<User, IEnumerable<string>>(async (user, columns) => user);
            service.Setup(x => x.Update(invalidUser, It.IsAny<IEnumerable<string>>())).Returns<User, IEnumerable<string>>(async (user, columns) => null);

            var controller = new UsersController(service.Object);

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
            LoginResponse invalidResponse = null;

            service.Setup(x => x.Login(validRequest)).ReturnsAsync(validResponse);
            service.Setup(x => x.Login(invalidRequest)).ReturnsAsync(invalidResponse);

            var controller = new UsersController(service.Object);

            var goodResult = await controller.Login(validRequest);
            var invalidResult = await controller.Login(invalidRequest);

            Assert.Equal(validResponse, goodResult.Value);

            Assert.Null(invalidResult.Value);
            Assert.IsType<BadRequestObjectResult>(invalidResult.Result);
        }
    }
}
