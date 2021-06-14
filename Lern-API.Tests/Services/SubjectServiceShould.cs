using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class SubjectServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Remove_Subject_And_Its_Children(Subject subject)
        {
            var context = TestSetup.SetupContext();

            await context.Subjects.AddAsync(subject);
            await context.SaveChangesAsync();
            
            context.Subjects.AsEnumerable().Should().NotBeEmpty();
            context.Modules.AsEnumerable().Should().NotBeEmpty();
            context.Concepts.AsEnumerable().Should().NotBeEmpty();
            context.Courses.AsEnumerable().Should().NotBeEmpty();
            context.Exercises.AsEnumerable().Should().NotBeEmpty();
            context.Questions.AsEnumerable().Should().NotBeEmpty();
            context.Answers.AsEnumerable().Should().NotBeEmpty();

            var service = new DatabaseService<Subject, SubjectRequest>(context, TestSetup.SetupHttpContext());
            var result = await service.Delete(subject.Id);

            result.Should().BeTrue();
            
            context.Subjects.AsEnumerable().Should().BeEmpty();
            context.Modules.AsEnumerable().Should().BeEmpty();
            context.Concepts.AsEnumerable().Should().BeEmpty();
            context.Courses.AsEnumerable().Should().BeEmpty();
            context.Exercises.AsEnumerable().Should().BeEmpty();
            context.Questions.AsEnumerable().Should().BeEmpty();
            context.Answers.AsEnumerable().Should().BeEmpty();
        }

        [Theory]
        [AutoMoqData]
        public async Task Remove_Subject_And_Related_Entities(User user, Subject subject, Result result, Progression progression)
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

            await context.Users.AddAsync(user);
            await context.Subjects.AddAsync(subject);
            await context.Progressions.AddAsync(progression);
            await context.Results.AddAsync(result);
            await context.SaveChangesAsync();
            
            context.Subjects.AsEnumerable().Should().NotBeEmpty();
            context.Modules.AsEnumerable().Should().NotBeEmpty();
            context.Concepts.AsEnumerable().Should().NotBeEmpty();
            context.Courses.AsEnumerable().Should().NotBeEmpty();
            context.Exercises.AsEnumerable().Should().NotBeEmpty();
            context.Questions.AsEnumerable().Should().NotBeEmpty();
            context.Answers.AsEnumerable().Should().NotBeEmpty();
            context.Progressions.AsEnumerable().Should().NotBeEmpty();
            context.Results.AsEnumerable().Should().NotBeEmpty();

            var service = new DatabaseService<Subject, SubjectRequest>(context, TestSetup.SetupHttpContext());
            var deleteResult = await service.Delete(subject.Id);

            deleteResult.Should().BeTrue();
            
            context.Subjects.AsEnumerable().Should().BeEmpty();
            context.Modules.AsEnumerable().Should().BeEmpty();
            context.Concepts.AsEnumerable().Should().BeEmpty();
            context.Courses.AsEnumerable().Should().BeEmpty();
            context.Exercises.AsEnumerable().Should().BeEmpty();
            context.Questions.AsEnumerable().Should().BeEmpty();
            context.Answers.AsEnumerable().Should().BeEmpty();
            context.Progressions.AsEnumerable().Should().BeEmpty();
            context.Results.AsEnumerable().Should().BeEmpty();
        }
    }
}
