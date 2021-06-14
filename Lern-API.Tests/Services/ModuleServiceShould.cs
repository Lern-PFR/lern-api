using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Services.Database;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Moq;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class ModuleServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_Entire_Module_With_Write_Access(Mock<IAuthorizationService> authorizationService, Module module, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, module, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            module.Concepts.First().Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Users.AddAsync(user);
            await context.Modules.AddRangeAsync(module);
            await context.SaveChangesAsync();

            var service = new ModuleService(context, httpContext, authorizationService.Object);
            var result = await service.Get(module.Id);

            result.Should().BeEquivalentTo(module);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Module_With_A_Course_And_An_Exercise(Mock<IAuthorizationService> authorizationService, Module module, Module invalidModule, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, It.IsAny<Module>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            module.Concepts.First().Exercises.First().Questions.First().Answers.First().Valid = true;

            invalidModule.Concepts.First().Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);
            invalidModule.Concepts.First().Courses.Clear();

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Modules.AddAsync(module);
            await context.Modules.AddAsync(invalidModule);
            await context.SaveChangesAsync();

            var service = new ModuleService(context, httpContext, authorizationService.Object);
            var result = await service.Get(module.Id);
            var invalidResult = await service.Get(invalidModule.Id);

            result.Should().NotBeNull().And.BeEquivalentTo(module);
            invalidResult.Should().BeNull();
        }
    }
}
