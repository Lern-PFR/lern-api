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
    public class CourseServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_Entire_Course_With_Write_Access(Mock<IAuthorizationService> authorizationService, IStateService stateService, Course course, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, course, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            course.Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Users.AddAsync(user);
            await context.Courses.AddRangeAsync(course);
            await context.SaveChangesAsync();

            var service = new CourseService(context, httpContext, authorizationService.Object, stateService);
            var result = await service.Get(course.Id);

            result.Should().BeEquivalentTo(course);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Course_With_A_Valid_Exercise(Mock<IAuthorizationService> authorizationService, IStateService stateService, Course course, Course invalidCourse, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, It.IsAny<Course>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            course.Exercises.First().Questions.First().Answers.First().Valid = true;

            invalidCourse.Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Courses.AddAsync(course);
            await context.Courses.AddAsync(invalidCourse);
            await context.SaveChangesAsync();

            var service = new CourseService(context, httpContext, authorizationService.Object, stateService);
            var result = await service.Get(course.Id);
            var invalidResult = await service.Get(invalidCourse.Id);

            result.Should().NotBeNull().And.BeEquivalentTo(course);
            invalidResult.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Single_Course_Or_Null(IAuthorizationService authorizationService, IStateService stateService, List<Course> entities, Course target)
        {
            var context = TestSetup.SetupContext();
            var service = new CourseService(context, TestSetup.SetupHttpContext(), authorizationService, stateService);

            await context.Courses.AddRangeAsync(entities);
            await context.Courses.AddAsync(target);
            await context.SaveChangesAsync();

            var result = await service.Get(target.Id);
            var invalidResult = await service.Get(Guid.Empty);

            result.Should().NotBeNull().And.BeEquivalentTo(target);
            invalidResult.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Latest_Version(IAuthorizationService authorizationService, IStateService stateService, List<Course> entities, Course target)
        {
            var request = new CourseRequest();
            request.CloneFrom(target);

            var context = TestSetup.SetupContext();
            var service = new CourseService(context, TestSetup.SetupHttpContext(), authorizationService, stateService);

            await context.Courses.AddRangeAsync(entities);
            await context.Courses.AddAsync(target);
            await context.SaveChangesAsync();

            var newCourse = await service.Update(target.Id, request);
            var result = await service.Get(target.Id);

            result.Should().NotBeNull().And.BeEquivalentTo(newCourse);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Exact_Version(IAuthorizationService authorizationService, IStateService stateService, List<Course> entities, Course target)
        {
            var delta = 1;

            entities.ForEach(x =>
            {
                x.Id = target.Id;
                x.Version = target.Version + delta;
                delta += 1;
            });

            var context = TestSetup.SetupContext();
            var service = new CourseService(context, TestSetup.SetupHttpContext(), authorizationService, stateService);

            await context.Courses.AddRangeAsync(entities);
            await context.Courses.AddAsync(target);
            await context.SaveChangesAsync();

            var result = await service.GetExact(target.Id, target.Version);

            result.Should().NotBeNull().And.BeEquivalentTo(target);
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Course_Version(IAuthorizationService authorizationService, IStateService stateService, Course entity, CourseRequest request)
        {
            var context = TestSetup.SetupContext();
            var service = new CourseService(context, TestSetup.SetupHttpContext(), authorizationService, stateService);

            await context.Courses.AddAsync(entity);
            await context.SaveChangesAsync();

            var expected = new Course();
            expected.CloneFrom(entity);
            expected.CloneFrom(request);
            expected.Version += 1;

            var result = await service.Update(entity.Id, request);

            result.Should().NotBeNull().And.BeEquivalentTo(expected, config => TestSetup.IgnoreTimestamps<Course>()(config.Excluding(course => course.Exercises)));
            context.Courses.Count().Should().Be(2);
        }

        [Theory]
        [AutoMoqData]
        public async Task Delete_All_Versions(IAuthorizationService authorizationService, IStateService stateService, List<Course> entities, Course target)
        {
            var context = TestSetup.SetupContext();
            var service = new CourseService(context, TestSetup.SetupHttpContext(), authorizationService, stateService);

            await context.Courses.AddRangeAsync(entities);

            var version = 0;

            for (var i = 0; i < 5; i++)
            {
                var newCourse = new Course();
                newCourse.CloneFrom(target);
                newCourse.Version = version;
                await context.Courses.AddAsync(target);
                version += 1;
            }

            await context.SaveChangesAsync();

            var result = await service.Delete(target.Id);

            result.Should().BeTrue();
            context.Courses.Any(x => x.Id == target.Id).Should().BeFalse();
            context.Courses.Count().Should().Be(entities.Count);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Update(Mock<IAuthorizationService> authorizationService, Mock<IStateService> stateService, Course course, CourseRequest request)
        {
            stateService.Setup(x => x.UpdateSubjectState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();
            
            await context.Courses.AddAsync(course);
            await context.SaveChangesAsync();

            var service = new CourseService(context, httpContext, authorizationService.Object, stateService.Object);
            await service.Update(course.Id, request);

            stateService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Create(Mock<IAuthorizationService> authorizationService, Mock<IStateService> stateService, CourseRequest request)
        {
            stateService.Setup(x => x.UpdateSubjectState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();
            
            var service = new CourseService(context, httpContext, authorizationService.Object, stateService.Object);
            await service.Create(request);

            stateService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Delete(Mock<IAuthorizationService> authorizationService, Mock<IStateService> stateService, Course course)
        {
            stateService.Setup(x => x.UpdateSubjectState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();

            await context.Courses.AddAsync(course);
            await context.SaveChangesAsync();

            var service = new CourseService(context, httpContext, authorizationService.Object, stateService.Object);
            await service.Delete(course.Id);

            stateService.VerifyAll();
        }
    }
}
