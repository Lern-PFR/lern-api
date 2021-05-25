using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.Controllers;
using Lern_API.DataTransferObjects.Requests;
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
        public async Task Return_Course_Or_404(Mock<IDatabaseService<Course, CourseRequest>> service, Course course, Guid goodGuid, Guid badGuid)
        {
            service.Setup(x => x.Get(goodGuid, It.IsAny<CancellationToken>())).ReturnsAsync(course);
            service.Setup(x => x.Get(badGuid, It.IsAny<CancellationToken>())).ReturnsAsync((Course) null);

            var controller = TestSetup.SetupController<CoursesController>(service.Object);

            var result = await controller.Get(goodGuid);
            var invalidResult = await controller.Get(badGuid);

            result.Value.Should().NotBeNull().And.BeEquivalentTo(course, TestSetup.IgnoreTimestamps<Course>());
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
