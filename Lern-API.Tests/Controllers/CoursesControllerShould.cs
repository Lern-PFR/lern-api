using System;
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
    public class CoursesControllerShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Return_Course_Or_404(Mock<IDatabaseService<Course, CourseRequest>> service, IAuthorizationService authorization, Course course, Guid goodGuid, Guid badGuid)
        {
            service.Setup(x => x.Get(goodGuid, It.IsAny<CancellationToken>())).ReturnsAsync(course);
            service.Setup(x => x.Get(badGuid, It.IsAny<CancellationToken>())).ReturnsAsync((Course) null);

            var controller = TestSetup.SetupController<CoursesController>(service.Object, authorization);

            var result = await controller.Get(goodGuid);
            var invalidResult = await controller.Get(badGuid);

            result.Value.Should().NotBeNull().And.BeEquivalentTo(course, TestSetup.IgnoreTimestamps<Course>());
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<NotFoundResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Create_Course_Or_409(Mock<IDatabaseService<Course, CourseRequest>> service, IAuthorizationService authorization, CourseRequest request, Course course, User user)
        {
            service.Setup(x => x.Create(request, It.IsAny<CancellationToken>())).ReturnsAsync(course);
            service.Setup(x => x.Create(null, It.IsAny<CancellationToken>())).ReturnsAsync((Course) null);

            var controller = TestSetup.SetupController<CoursesController>(service.Object, authorization).SetupSession(user);

            var goodResult = await controller.Create(request);
            var invalidResult = await controller.Create(null);

            goodResult.Value.Should().NotBeNull();
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<ConflictResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Course_Or_409(Mock<IDatabaseService<Course, CourseRequest>> service, Mock<IAuthorizationService> authorization, CourseRequest validRequest, CourseRequest invalidRequest, Course valid, Course invalid, User user)
        {
            authorization.Setup(x => x.HasWriteAccess(user, It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            service.Setup(x => x.Exists(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Get(valid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(valid);
            service.Setup(x => x.Get(invalid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invalid);
            service.Setup(x => x.Update(valid.Id, validRequest, It.IsAny<CancellationToken>())).ReturnsAsync(valid);
            service.Setup(x => x.Update(invalid.Id, invalidRequest, It.IsAny<CancellationToken>())).ReturnsAsync((Course) null);

            var controller = TestSetup.SetupController<CoursesController>(service.Object, authorization.Object).SetupSession(user);

            var goodResult = await controller.Update(valid.Id, validRequest);
            var invalidResult = await controller.Update(invalid.Id, invalidRequest);

            goodResult.Value.Should().BeEquivalentTo(valid);
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<ConflictResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Course_Or_404(Mock<IDatabaseService<Course, CourseRequest>> service, Mock<IAuthorizationService> authorization, CourseRequest validRequest, CourseRequest invalidRequest, Course valid, Course invalid, User user)
        {
            authorization.Setup(x => x.HasWriteAccess(user, It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            service.Setup(x => x.Exists(valid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Exists(invalid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            service.Setup(x => x.Get(valid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(valid);
            service.Setup(x => x.Get(invalid.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Course) null);
            service.Setup(x => x.Update(valid.Id, validRequest, It.IsAny<CancellationToken>())).ReturnsAsync(valid);
            service.Setup(x => x.Update(invalid.Id, invalidRequest, It.IsAny<CancellationToken>())).ReturnsAsync((Course) null);

            var controller = TestSetup.SetupController<CoursesController>(service.Object, authorization.Object).SetupSession(user);

            var goodResult = await controller.Update(valid.Id, validRequest);
            var invalidResult = await controller.Update(invalid.Id, invalidRequest);

            goodResult.Value.Should().BeEquivalentTo(valid);
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<NotFoundResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Course_Or_401(Mock<IDatabaseService<Course, CourseRequest>> service, Mock<IAuthorizationService> authorization, CourseRequest validRequest, CourseRequest invalidRequest, Course valid, Course invalid, User user)
        {
            authorization.Setup(x => x.HasWriteAccess(user, valid, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            authorization.Setup(x => x.HasWriteAccess(user, invalid, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            service.Setup(x => x.Exists(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Get(valid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(valid);
            service.Setup(x => x.Get(invalid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invalid);
            service.Setup(x => x.Update(valid.Id, validRequest, It.IsAny<CancellationToken>())).ReturnsAsync(valid);
            service.Setup(x => x.Update(invalid.Id, invalidRequest, It.IsAny<CancellationToken>())).ReturnsAsync(invalid);

            var controller = TestSetup.SetupController<CoursesController>(service.Object, authorization.Object).SetupSession(user);

            var goodResult = await controller.Update(valid.Id, validRequest);
            var invalidResult = await controller.Update(invalid.Id, invalidRequest);

            goodResult.Value.Should().BeEquivalentTo(valid);
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Delete_Course_Or_500(Mock<IDatabaseService<Course, CourseRequest>> service, Mock<IAuthorizationService> authorization, Course valid, Course invalid, User user)
        {
            authorization.Setup(x => x.HasAuthorship(user, It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            service.Setup(x => x.Exists(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Get(valid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(valid);
            service.Setup(x => x.Get(invalid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invalid);
            service.Setup(x => x.Delete(valid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Delete(invalid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var controller = TestSetup.SetupController<CoursesController>(service.Object, authorization.Object).SetupSession(user);

            var goodResult = await controller.Delete(valid.Id);
            var invalidResult = await controller.Delete(invalid.Id);

            goodResult.Value.Should().BeEquivalentTo(valid);
            invalidResult.Result.Should().BeOfType<ObjectResult>();

            var objectResult = (ObjectResult) invalidResult.Result;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeOfType<ErrorResponse>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Delete_Course_Or_404(Mock<IDatabaseService<Course, CourseRequest>> service, Mock<IAuthorizationService> authorization, Course valid, Course invalid, User user)
        {
            authorization.Setup(x => x.HasAuthorship(user, It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            service.Setup(x => x.Exists(valid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Exists(invalid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            service.Setup(x => x.Get(valid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(valid);
            service.Setup(x => x.Get(invalid.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Course) null);
            service.Setup(x => x.Delete(valid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Delete(invalid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var controller = TestSetup.SetupController<CoursesController>(service.Object, authorization.Object).SetupSession(user);

            var goodResult = await controller.Delete(valid.Id);
            var invalidResult = await controller.Delete(invalid.Id);

            goodResult.Value.Should().BeEquivalentTo(valid);
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<NotFoundResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Delete_Course_Or_401(Mock<IDatabaseService<Course, CourseRequest>> service, Mock<IAuthorizationService> authorization, Course valid, Course invalid, User user)
        {
            authorization.Setup(x => x.HasAuthorship(user, valid, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            authorization.Setup(x => x.HasAuthorship(user, invalid, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            service.Setup(x => x.Exists(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Get(valid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(valid);
            service.Setup(x => x.Get(invalid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invalid);
            service.Setup(x => x.Delete(valid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Delete(invalid.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var controller = TestSetup.SetupController<CoursesController>(service.Object, authorization.Object).SetupSession(user);

            var goodResult = await controller.Delete(valid.Id);
            var invalidResult = await controller.Delete(invalid.Id);

            goodResult.Value.Should().BeEquivalentTo(valid);
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}
