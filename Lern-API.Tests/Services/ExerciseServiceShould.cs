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
    public class ExerciseServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_Entire_Exercise_With_Write_Access(Mock<IAuthorizationService> authorizationService, Exercise exercise, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, exercise, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            exercise.Questions.First().Answers.ForEach(x => x.Valid = false);

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Users.AddAsync(user);
            await context.Exercises.AddRangeAsync(exercise);
            await context.SaveChangesAsync();

            var service = new ExerciseService(context, httpContext, authorizationService.Object);
            var result = await service.Get(exercise.Id);

            result.Should().BeEquivalentTo(exercise);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Exercise_With_A_Valid_Question(Mock<IAuthorizationService> authorizationService, Exercise exercise, Exercise invalidExercise, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, It.IsAny<Exercise>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            
            invalidExercise.Questions.First().Answers.ForEach(x => x.Valid = false);

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Exercises.AddAsync(exercise);
            await context.Exercises.AddAsync(invalidExercise);
            await context.SaveChangesAsync();

            var service = new ExerciseService(context, httpContext, authorizationService.Object);
            var result = await service.Get(exercise.Id);
            var invalidResult = await service.Get(invalidExercise.Id);

            result.Should().NotBeNull().And.BeEquivalentTo(exercise);
            invalidResult.Should().BeNull();
        }
    }
}
