using System;
using System.Collections.Generic;
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
    public class SubjectsControllerShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Return_All_Subjects(Mock<IDatabaseService<Subject, SubjectRequest>> service, List<Subject> subjects)
        {
            service.Setup(x => x.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(subjects);

            var controller = TestSetup.SetupController<SubjectsController>(service.Object);

            var result = await controller.GetAll();
            
            result.Should().NotBeNull().And.BeEquivalentTo(subjects, TestSetup.IgnoreTimestamps<Subject>());
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Subject_Or_404(Mock<IDatabaseService<Subject, SubjectRequest>> service, Subject subject, Guid goodGuid, Guid badGuid)
        {
            service.Setup(x => x.Get(goodGuid, It.IsAny<CancellationToken>())).ReturnsAsync(subject);
            service.Setup(x => x.Get(badGuid, It.IsAny<CancellationToken>())).ReturnsAsync((Subject) null);

            var controller = TestSetup.SetupController<SubjectsController>(service.Object);

            var result = await controller.Get(goodGuid);
            var invalidResult = await controller.Get(badGuid);

            result.Value.Should().NotBeNull().And.BeEquivalentTo(subject, TestSetup.IgnoreTimestamps<Subject>());
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
