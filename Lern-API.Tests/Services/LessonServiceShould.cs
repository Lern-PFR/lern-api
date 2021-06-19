using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Helpers.Models;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Services.Database;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Moq;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class LessonServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_Entire_Lesson_With_Write_Access(Mock<IAuthorizationService> authorizationService, IStateService stateService, Lesson lesson, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, lesson, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            lesson.Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Users.AddAsync(user);
            await context.Lessons.AddRangeAsync(lesson);
            await context.SaveChangesAsync();

            var service = new LessonService(context, httpContext, authorizationService.Object, stateService);
            var result = await service.Get(lesson.Id);

            result.Should().BeEquivalentTo(lesson);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Lesson_With_A_Valid_Exercise(Mock<IAuthorizationService> authorizationService, IStateService stateService, Lesson lesson, Lesson invalidLesson, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, It.IsAny<Lesson>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            lesson.Exercises.First().Questions.First().Answers.First().Valid = true;

            invalidLesson.Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Lessons.AddAsync(lesson);
            await context.Lessons.AddAsync(invalidLesson);
            await context.SaveChangesAsync();

            var service = new LessonService(context, httpContext, authorizationService.Object, stateService);
            var result = await service.Get(lesson.Id);
            var invalidResult = await service.Get(invalidLesson.Id);

            result.Should().NotBeNull().And.BeEquivalentTo(lesson);
            invalidResult.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Single_Lesson_Or_Null(IAuthorizationService authorizationService, IStateService stateService, List<Lesson> entities, Lesson target)
        {
            var context = TestSetup.SetupContext();
            var service = new LessonService(context, TestSetup.SetupHttpContext(), authorizationService, stateService);

            await context.Lessons.AddRangeAsync(entities);
            await context.Lessons.AddAsync(target);
            await context.SaveChangesAsync();

            var result = await service.Get(target.Id);
            var invalidResult = await service.Get(Guid.Empty);

            result.Should().NotBeNull().And.BeEquivalentTo(target);
            invalidResult.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Latest_Version(IAuthorizationService authorizationService, IStateService stateService, List<Lesson> entities, Lesson target)
        {
            var request = new LessonRequest();
            request.CloneFrom(target);

            var context = TestSetup.SetupContext();
            var service = new LessonService(context, TestSetup.SetupHttpContext(), authorizationService, stateService);

            await context.Lessons.AddRangeAsync(entities);
            await context.Lessons.AddAsync(target);
            await context.SaveChangesAsync();

            var newLesson = await service.Update(target.Id, request);
            var result = await service.Get(target.Id);

            result.Should().NotBeNull().And.BeEquivalentTo(newLesson);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Exact_Version(IAuthorizationService authorizationService, IStateService stateService, List<Lesson> entities, Lesson target)
        {
            var delta = 1;

            entities.ForEach(x =>
            {
                x.Id = target.Id;
                x.Version = target.Version + delta;
                delta += 1;
            });

            var context = TestSetup.SetupContext();
            var service = new LessonService(context, TestSetup.SetupHttpContext(), authorizationService, stateService);

            await context.Lessons.AddRangeAsync(entities);
            await context.Lessons.AddAsync(target);
            await context.SaveChangesAsync();

            var result = await service.GetExact(target.Id, target.Version);

            result.Should().NotBeNull().And.BeEquivalentTo(target);
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Lesson_Version(IAuthorizationService authorizationService, IStateService stateService, Lesson entity, LessonRequest request)
        {
            var context = TestSetup.SetupContext();
            var service = new LessonService(context, TestSetup.SetupHttpContext(), authorizationService, stateService);

            await context.Lessons.AddAsync(entity);
            await context.SaveChangesAsync();

            var expected = new Lesson();
            expected.CloneFrom(entity);
            expected.CloneFrom(request);
            expected.Version += 1;

            var result = await service.Update(entity.Id, request);

            result.Should().NotBeNull().And.BeEquivalentTo(expected, config => TestSetup.IgnoreTimestamps<Lesson>()(config.Excluding(lesson => lesson.Exercises)));
            context.Lessons.Count().Should().Be(2);
        }

        [Theory]
        [AutoMoqData]
        public async Task Delete_All_Versions(IAuthorizationService authorizationService, IStateService stateService, List<Lesson> entities, Lesson target)
        {
            var context = TestSetup.SetupContext();
            var service = new LessonService(context, TestSetup.SetupHttpContext(), authorizationService, stateService);

            await context.Lessons.AddRangeAsync(entities);

            var version = 0;

            for (var i = 0; i < 5; i++)
            {
                var newLesson = new Lesson();
                newLesson.CloneFrom(target);
                newLesson.Version = version;
                await context.Lessons.AddAsync(target);
                version += 1;
            }

            await context.SaveChangesAsync();

            var result = await service.Delete(target.Id);

            result.Should().BeTrue();
            context.Lessons.Any(x => x.Id == target.Id).Should().BeFalse();
            context.Lessons.Count().Should().Be(entities.Count);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Update(Mock<IAuthorizationService> authorizationService, Mock<IStateService> stateService, Lesson lesson, LessonRequest request)
        {
            stateService.Setup(x => x.UpdateSubjectState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();
            
            await context.Lessons.AddAsync(lesson);
            await context.SaveChangesAsync();

            var service = new LessonService(context, httpContext, authorizationService.Object, stateService.Object);
            await service.Update(lesson.Id, request);

            stateService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Create(Mock<IAuthorizationService> authorizationService, Mock<IStateService> stateService, LessonRequest request)
        {
            stateService.Setup(x => x.UpdateSubjectState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();
            
            var service = new LessonService(context, httpContext, authorizationService.Object, stateService.Object);
            await service.Create(request);

            stateService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Delete(Mock<IAuthorizationService> authorizationService, Mock<IStateService> stateService, Lesson lesson)
        {
            stateService.Setup(x => x.UpdateSubjectState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();

            await context.Lessons.AddAsync(lesson);
            await context.SaveChangesAsync();

            var service = new LessonService(context, httpContext, authorizationService.Object, stateService.Object);
            await service.Delete(lesson.Id);

            stateService.VerifyAll();
        }
    }
}
