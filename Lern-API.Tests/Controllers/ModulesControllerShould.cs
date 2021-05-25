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
    public class ModulesControllerShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Return_Module_Or_404(Mock<IDatabaseService<Module, ModuleRequest>> service, Module module, Guid goodGuid, Guid badGuid)
        {
            service.Setup(x => x.Get(goodGuid, It.IsAny<CancellationToken>())).ReturnsAsync(module);
            service.Setup(x => x.Get(badGuid, It.IsAny<CancellationToken>())).ReturnsAsync((Module) null);

            var controller = TestSetup.SetupController<ModulesController>(service.Object);

            var result = await controller.Get(goodGuid);
            var invalidResult = await controller.Get(badGuid);

            result.Value.Should().NotBeNull().And.BeEquivalentTo(module, TestSetup.IgnoreTimestamps<Module>());
            invalidResult.Value.Should().BeNull();
            invalidResult.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
