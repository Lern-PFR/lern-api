using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.DataTransferObjects.Requests;
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
        public async Task Get_Entire_Module_With_Write_Access(Mock<IAuthorizationService> authorizationService, IStateService stateService, Module module, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, module, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            module.Concepts.First().Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Users.AddAsync(user);
            await context.Modules.AddAsync(module);
            await context.SaveChangesAsync();

            var service = new ModuleService(context, httpContext, authorizationService.Object, stateService);
            var result = await service.Get(module.Id);

            result.Should().BeEquivalentTo(module);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Module_With_A_Lesson_And_An_Exercise(Mock<IAuthorizationService> authorizationService, IStateService stateService, Module module, Module invalidModule, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, It.IsAny<Module>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            module.Concepts.First().Exercises.First().Questions.First().Answers.First().Valid = true;

            invalidModule.Concepts.First().Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);
            invalidModule.Concepts.First().Lessons.Clear();

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Modules.AddAsync(module);
            await context.Modules.AddAsync(invalidModule);
            await context.SaveChangesAsync();

            var service = new ModuleService(context, httpContext, authorizationService.Object, stateService);
            var result = await service.Get(module.Id);
            var invalidResult = await service.Get(invalidModule.Id);

            result.Should().NotBeNull().And.BeEquivalentTo(module);
            invalidResult.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Update(Mock<IAuthorizationService> authorizationService, Mock<IStateService> stateService, Module module, ModuleRequest request)
        {
            stateService.Setup(x => x.UpdateSubjectState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();
            
            await context.Modules.AddAsync(module);
            await context.SaveChangesAsync();

            var service = new ModuleService(context, httpContext, authorizationService.Object, stateService.Object);
            await service.Update(module.Id, request);

            stateService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Create(Mock<IAuthorizationService> authorizationService, Mock<IStateService> stateService, ModuleRequest request)
        {
            stateService.Setup(x => x.UpdateSubjectState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();
            
            var service = new ModuleService(context, httpContext, authorizationService.Object, stateService.Object);
            await service.Create(request);

            stateService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Delete(Mock<IAuthorizationService> authorizationService, Mock<IStateService> stateService, Module module)
        {
            stateService.Setup(x => x.UpdateSubjectState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();

            await context.Modules.AddAsync(module);
            await context.SaveChangesAsync();

            var service = new ModuleService(context, httpContext, authorizationService.Object, stateService.Object);
            await service.Delete(module.Id);

            stateService.VerifyAll();
        }
    }
}
