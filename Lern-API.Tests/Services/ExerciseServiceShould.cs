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
    public class ExerciseServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_Entire_Exercise_With_Write_Access(Mock<IAuthorizationService> authorizationService, ISubjectService subjectService, Exercise exercise, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, exercise, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            exercise.Questions.First().Answers.ForEach(x => x.Valid = false);

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Users.AddAsync(user);
            await context.Exercises.AddRangeAsync(exercise);
            await context.SaveChangesAsync();

            var service = new ExerciseService(context, httpContext, authorizationService.Object, subjectService);
            var result = await service.Get(exercise.Id);

            result.Should().BeEquivalentTo(exercise);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Exercise_With_A_Valid_Question(Mock<IAuthorizationService> authorizationService, ISubjectService subjectService, Exercise exercise, Exercise invalidExercise, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, It.IsAny<Exercise>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            
            invalidExercise.Questions.First().Answers.ForEach(x => x.Valid = false);

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Exercises.AddAsync(exercise);
            await context.Exercises.AddAsync(invalidExercise);
            await context.SaveChangesAsync();

            var service = new ExerciseService(context, httpContext, authorizationService.Object, subjectService);
            var result = await service.Get(exercise.Id);
            var invalidResult = await service.Get(invalidExercise.Id);

            result.Should().NotBeNull().And.BeEquivalentTo(exercise);
            invalidResult.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Update(Mock<IAuthorizationService> authorizationService, Mock<ISubjectService> subjectService, Exercise exercise, ExerciseRequest request)
        {
            subjectService.Setup(x => x.UpdateState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();
            
            await context.Exercises.AddAsync(exercise);
            await context.SaveChangesAsync();

            var service = new ExerciseService(context, httpContext, authorizationService.Object, subjectService.Object);
            await service.Update(exercise.Id, request);

            subjectService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Create(Mock<IAuthorizationService> authorizationService, Mock<ISubjectService> subjectService, ExerciseRequest request)
        {
            subjectService.Setup(x => x.UpdateState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();
            
            var service = new ExerciseService(context, httpContext, authorizationService.Object, subjectService.Object);
            await service.Create(request);

            subjectService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Delete(Mock<IAuthorizationService> authorizationService, Mock<ISubjectService> subjectService, Exercise exercise)
        {
            subjectService.Setup(x => x.UpdateState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();

            await context.Exercises.AddAsync(exercise);
            await context.SaveChangesAsync();

            var service = new ExerciseService(context, httpContext, authorizationService.Object, subjectService.Object);
            await service.Delete(exercise.Id);

            subjectService.VerifyAll();
        }
    }
}
