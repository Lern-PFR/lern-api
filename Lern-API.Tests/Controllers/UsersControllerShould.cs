﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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

            var expected = users.Select(user => new UserResponse(user));

            var controller = TestSetup.SetupController<UsersController>(service.Object);

            var result = await controller.GetAllUsers();
            
            result.Should().NotBeNull().And.BeEquivalentTo(expected);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_User_Or_404(Mock<IUserService> service, User user, Guid goodGuid, Guid badGuid)
        {
            service.Setup(x => x.Get(goodGuid, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            service.Setup(x => x.Get(badGuid, It.IsAny<CancellationToken>())).ReturnsAsync((User) null);

            var expected = new UserResponse(user);

            var controller = TestSetup.SetupController<UsersController>(service.Object);

            var result = await controller.GetUser(goodGuid);
            var invalidResult = await controller.GetUser(badGuid);

            result.Value.Should().NotBeNull().And.BeEquivalentTo(expected);
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<NotFoundResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Create_User_Or_409(Mock<IUserService> service, UserRequest request, User user)
        {
            service.Setup(x => x.Create(request, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            service.Setup(x => x.Create(null, It.IsAny<CancellationToken>())).ReturnsAsync((User) null);

            var controller = TestSetup.SetupController<UsersController>(service.Object);

            var goodUser = await controller.CreateUser(request);
            var invalidUser = await controller.CreateUser(null);

            goodUser.Value.Should().NotBeNull();
            invalidUser.Value.Should().BeNull();
            invalidUser.Result.Should().BeOfType<ConflictResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_User_Or_409(Mock<IUserService> service, UserRequest validRequest, UserRequest invalidRequest, User validUser, User invalidUser)
        {
            validUser.Admin = true;
            
            service.Setup(x => x.Exists(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Update(validUser.Id, validRequest, It.IsAny<CancellationToken>())).ReturnsAsync(validUser);
            service.Setup(x => x.Update(invalidUser.Id, invalidRequest, It.IsAny<CancellationToken>())).ReturnsAsync((User) null);

            var controller = TestSetup.SetupController<UsersController>(service.Object).SetupSession(validUser);

            var goodResult = await controller.UpdateUser(validUser.Id, validRequest);
            var invalidResult = await controller.UpdateUser(invalidUser.Id, invalidRequest);

            goodResult.Value.Should().BeEquivalentTo(validUser);
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<ConflictResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_User_Or_404(Mock<IUserService> service, UserRequest validRequest, UserRequest invalidRequest, User validUser, User invalidUser)
        {
            validUser.Admin = true;
            
            service.Setup(x => x.Exists(validUser.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Exists(invalidUser.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            service.Setup(x => x.Update(validUser.Id, validRequest, It.IsAny<CancellationToken>())).ReturnsAsync(validUser);
            service.Setup(x => x.Update(invalidUser.Id, invalidRequest, It.IsAny<CancellationToken>())).ReturnsAsync((User) null);

            var controller = TestSetup.SetupController<UsersController>(service.Object).SetupSession(validUser);

            var goodResult = await controller.UpdateUser(validUser.Id, validRequest);
            var invalidResult = await controller.UpdateUser(invalidUser.Id, invalidRequest);

            goodResult.Value.Should().BeEquivalentTo(validUser);
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<NotFoundResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_User_Or_401(Mock<IUserService> service, UserRequest validRequest, UserRequest invalidRequest, User validUser, User invalidUser)
        {
            validUser.Admin = false;
            
            service.Setup(x => x.Exists(validUser.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Exists(invalidUser.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            service.Setup(x => x.Update(validUser.Id, validRequest, It.IsAny<CancellationToken>())).ReturnsAsync(validUser);
            service.Setup(x => x.Update(invalidUser.Id, invalidRequest, It.IsAny<CancellationToken>())).ReturnsAsync((User) null);

            var controller = TestSetup.SetupController<UsersController>(service.Object).SetupSession(validUser);

            var goodResult = await controller.UpdateUser(validUser.Id, validRequest);
            var invalidResult = await controller.UpdateUser(invalidUser.Id, invalidRequest);

            goodResult.Value.Should().BeEquivalentTo(validUser);
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<UnauthorizedResult>();
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
            
            goodResult.Value.Should().Be(validResponse);

            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [AutoMoqData]
        public void Return_Current_User(Mock<IUserService> service, User user)
        {
            var controller = TestSetup.SetupController<UsersController>(service.Object).SetupSession(user);

            var result = controller.Whoami();

            result.Should().NotBeNull();
            result.Value.Should().Be(user);
        }
    }
}
