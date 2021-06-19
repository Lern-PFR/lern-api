using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.Filters;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Services.Database;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Moq;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class SubjectServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_Entire_Subject_With_Write_Access(Mock<IAuthorizationService> authorizationService, IProgressionService progressionService, IStateService stateService, Subject subject, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            subject.Modules.First().Concepts.First().Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Users.AddAsync(user);
            await context.Subjects.AddRangeAsync(subject);
            await context.SaveChangesAsync();

            var service = new SubjectService(context, httpContext, authorizationService.Object, progressionService, stateService);
            var result = await service.Get(subject.Id);

            result.Should().BeEquivalentTo(subject);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Subject_With_A_Lesson_And_An_Exercise(Mock<IAuthorizationService> authorizationService, IProgressionService progressionService, Mock<IStateService> stateService, Subject subject, Subject invalidSubject, User user)
        {
            var context = TestSetup.SetupContext();

            authorizationService.Setup(x => x.HasWriteAccess(user, It.IsAny<Subject>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            stateService.Setup(x => x.AvailableSubjects).Returns(SubjectsFilters.ValidSubjects(context.Subjects));

            subject.Modules.First().Concepts.First().Exercises.First().Questions.First().Answers.First().Valid = true;

            invalidSubject.Modules.First().Concepts.First().Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);
            invalidSubject.Modules.First().Concepts.First().Lessons.Clear();
            
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Subjects.AddAsync(subject);
            await context.Subjects.AddAsync(invalidSubject);
            await context.SaveChangesAsync();

            var service = new SubjectService(context, httpContext, authorizationService.Object, progressionService, stateService.Object);
            var result = await service.Get(subject.Id);
            var invalidResult = await service.Get(invalidSubject.Id);

            result.Should().NotBeNull().And.BeEquivalentTo(subject);
            invalidResult.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_All_Subjects_With_A_Module(IAuthorizationService authorizationService, IProgressionService progressionService, Mock<IStateService> stateService, List<Subject> subjects, List<Subject> invalidSubjects, User user)
        {
            invalidSubjects.ForEach(x => x.Modules.Clear());

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            stateService.Setup(x => x.AvailableSubjects).Returns(SubjectsFilters.ValidSubjects(context.Subjects));

            await context.Subjects.AddRangeAsync(subjects);
            await context.Subjects.AddRangeAsync(invalidSubjects);
            await context.SaveChangesAsync();

            var service = new SubjectService(context, httpContext, authorizationService, progressionService, stateService.Object);
            var result = await service.GetAvailable();

            result.Should().BeEquivalentTo(subjects).And.NotBeEquivalentTo(invalidSubjects);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_All_Subjects_With_A_Concept(IAuthorizationService authorizationService, IProgressionService progressionService, Mock<IStateService> stateService, List<Subject> subjects, List<Subject> invalidSubjects, User user)
        {
            invalidSubjects.ForEach(x => x.Modules.ForEach(y => y.Concepts.Clear()));

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            stateService.Setup(x => x.AvailableSubjects).Returns(SubjectsFilters.ValidSubjects(context.Subjects));

            await context.Subjects.AddRangeAsync(subjects);
            await context.Subjects.AddRangeAsync(invalidSubjects);
            await context.SaveChangesAsync();

            var service = new SubjectService(context, httpContext, authorizationService, progressionService, stateService.Object);
            var result = await service.GetAvailable();

            result.Should().BeEquivalentTo(subjects).And.NotBeEquivalentTo(invalidSubjects);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_All_Subjects_With_A_Lesson_And_Exercise(IAuthorizationService authorizationService, IProgressionService progressionService, Mock<IStateService> stateService, List<Subject> subjects, List<Subject> invalidSubjects, User user)
        {
            invalidSubjects.ForEach(x => x.Modules.ForEach(y => y.Concepts.ForEach(concept =>
            {
                concept.Lessons.Clear();
                concept.Exercises.Clear();
            })));

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            stateService.Setup(x => x.AvailableSubjects).Returns(SubjectsFilters.ValidSubjects(context.Subjects));

            await context.Subjects.AddRangeAsync(subjects);
            await context.Subjects.AddRangeAsync(invalidSubjects);
            await context.SaveChangesAsync();

            var service = new SubjectService(context, httpContext, authorizationService, progressionService, stateService.Object);
            var result = await service.GetAvailable();

            result.Should().BeEquivalentTo(subjects).And.NotBeEquivalentTo(invalidSubjects);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_All_Subjects_From_User(IAuthorizationService authorizationService, IProgressionService progressionService, Mock<IStateService> stateService, User user, List<Subject> subjects)
        {
            var context = TestSetup.SetupContext();

            stateService.Setup(x => x.AvailableSubjects).Returns(SubjectsFilters.ValidSubjects(context.Subjects));

            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            subjects = subjects.Select((subject, i) =>
            {
                if (i % 2 == 0)
                    return subject;

                subject.AuthorId = user.Id;
                subject.Author = user;
                return subject;
            }).ToList();

            await context.Users.AddAsync(user);
            await context.Subjects.AddRangeAsync(subjects);
            await context.SaveChangesAsync();

            var service = new SubjectService(context, httpContext, authorizationService, progressionService, stateService.Object);
            var result = await service.GetMine();

            result.Should().BeEquivalentTo(subjects.Where(x => x.AuthorId == user.Id));
        }

        [Theory]
        [AutoMoqData]
        public async Task Remove_Subject_And_Its_Children(IAuthorizationService authorizationService, IProgressionService progressionService, Mock<IStateService> stateService, Subject subject, User user)
        {
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            stateService.Setup(x => x.AvailableSubjects).Returns(SubjectsFilters.ValidSubjects(context.Subjects));

            await context.Subjects.AddAsync(subject);
            await context.SaveChangesAsync();
            
            context.Subjects.AsEnumerable().Should().NotBeEmpty();
            context.Modules.AsEnumerable().Should().NotBeEmpty();
            context.Concepts.AsEnumerable().Should().NotBeEmpty();
            context.Lessons.AsEnumerable().Should().NotBeEmpty();
            context.Exercises.AsEnumerable().Should().NotBeEmpty();
            context.Questions.AsEnumerable().Should().NotBeEmpty();
            context.Answers.AsEnumerable().Should().NotBeEmpty();

            var service = new SubjectService(context, httpContext, authorizationService, progressionService, stateService.Object);
            var result = await service.Delete(subject.Id);

            result.Should().BeTrue();
            
            context.Subjects.AsEnumerable().Should().BeEmpty();
            context.Modules.AsEnumerable().Should().BeEmpty();
            context.Concepts.AsEnumerable().Should().BeEmpty();
            context.Lessons.AsEnumerable().Should().BeEmpty();
            context.Exercises.AsEnumerable().Should().BeEmpty();
            context.Questions.AsEnumerable().Should().BeEmpty();
            context.Answers.AsEnumerable().Should().BeEmpty();
        }

        [Theory]
        [AutoMoqData]
        public async Task Remove_Subject_And_Related_Entities(IAuthorizationService authorizationService, IProgressionService progressionService, Mock<IStateService> stateService, User user, Subject subject, Result result, Progression progression)
        {
            var concept = subject.Modules.First().Concepts.First();
            var question = concept.Exercises.First().Questions.First();
            var answer = question.Answers.First();

            result.AnswerId = answer.Id;
            result.Answer = answer;
            result.UserId = user.Id;
            result.User = user;
            result.QuestionId = question.Id;
            result.Question = question;

            progression.SubjectId = subject.Id;
            progression.Subject = subject;
            progression.UserId = user.Id;
            progression.User = user;
            progression.ConceptId = concept.Id;
            progression.Concept = concept;

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            stateService.Setup(x => x.AvailableSubjects).Returns(SubjectsFilters.ValidSubjects(context.Subjects));

            await context.Users.AddAsync(user);
            await context.Subjects.AddAsync(subject);
            await context.Progressions.AddAsync(progression);
            await context.Results.AddAsync(result);
            await context.SaveChangesAsync();
            
            context.Subjects.AsEnumerable().Should().NotBeEmpty();
            context.Modules.AsEnumerable().Should().NotBeEmpty();
            context.Concepts.AsEnumerable().Should().NotBeEmpty();
            context.Lessons.AsEnumerable().Should().NotBeEmpty();
            context.Exercises.AsEnumerable().Should().NotBeEmpty();
            context.Questions.AsEnumerable().Should().NotBeEmpty();
            context.Answers.AsEnumerable().Should().NotBeEmpty();
            context.Progressions.AsEnumerable().Should().NotBeEmpty();
            context.Results.AsEnumerable().Should().NotBeEmpty();

            var service = new SubjectService(context, httpContext, authorizationService, progressionService, stateService.Object);
            var deleteResult = await service.Delete(subject.Id);

            deleteResult.Should().BeTrue();
            
            context.Subjects.AsEnumerable().Should().BeEmpty();
            context.Modules.AsEnumerable().Should().BeEmpty();
            context.Concepts.AsEnumerable().Should().BeEmpty();
            context.Lessons.AsEnumerable().Should().BeEmpty();
            context.Exercises.AsEnumerable().Should().BeEmpty();
            context.Questions.AsEnumerable().Should().BeEmpty();
            context.Answers.AsEnumerable().Should().BeEmpty();
            context.Progressions.AsEnumerable().Should().BeEmpty();
            context.Results.AsEnumerable().Should().BeEmpty();
        }
    }
}
