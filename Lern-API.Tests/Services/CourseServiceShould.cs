using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Helpers.Models;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Services.Database;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class CourseServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_Single_Course_Or_Null(List<Course> entities, Course target)
        {
            var context = TestSetup.SetupContext();
            var service = new CourseService(context, TestSetup.SetupHttpContext());

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
        public async Task Get_Latest_Version(List<Course> entities, Course target)
        {
            var newCourse = new Course();
            newCourse.CloneFrom(target);
            newCourse.Version += 1;

            var context = TestSetup.SetupContext();
            var service = new CourseService(context, TestSetup.SetupHttpContext());

            await context.Courses.AddRangeAsync(entities);
            await context.Courses.AddAsync(target);
            await context.Courses.AddAsync(newCourse);
            await context.SaveChangesAsync();

            var result = await service.Get(target.Id);

            result.Should().NotBeNull().And.BeEquivalentTo(newCourse);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Exact_Version(List<Course> entities, Course target)
        {
            var delta = 1;

            entities.ForEach(x =>
            {
                x.Id = target.Id;
                x.Version = target.Version + delta;
                delta += 1;
            });

            var context = TestSetup.SetupContext();
            var service = new CourseService(context, TestSetup.SetupHttpContext());

            await context.Courses.AddRangeAsync(entities);
            await context.Courses.AddAsync(target);
            await context.SaveChangesAsync();

            var result = await service.GetExact(target.Id, target.Version);

            result.Should().NotBeNull().And.BeEquivalentTo(target);
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Course_Version(Course entity, CourseRequest request)
        {
            var context = TestSetup.SetupContext();
            var service = new CourseService(context, TestSetup.SetupHttpContext());

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
        public async Task Delete_All_Versions(List<Course> entities, Course target)
        {
            var context = TestSetup.SetupContext();
            var service = new CourseService(context, TestSetup.SetupHttpContext());

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
    }
}
