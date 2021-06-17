using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Models;
using Lern_API.Services.Database;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class ResultServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_All_From_Subject(User user, Subject subject, List<Result> otherResults, List<Result> randomResults)
        {
            var questions = subject.Modules.First().Concepts.First().Exercises.First().Questions;
            questions.AddRange(subject.Modules.Last().Concepts.Last().Exercises.Last().Questions);

            var entities = questions.Select(question => new Result
            {
                QuestionId = question.Id,
                AnswerId = question.Answers.First().Id,
                UserId = user.Id
            }).ToList();

            otherResults.ForEach(x => x.UserId = user.Id);

            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.Subjects.AddAsync(subject);
            await context.Results.AddRangeAsync(entities);
            await context.Results.AddRangeAsync(otherResults);
            await context.Results.AddRangeAsync(randomResults);
            await context.SaveChangesAsync();

            var service = new ResultService(context);

            var result = await service.GetAll(user, subject);

            result.Should().NotBeNullOrEmpty().And.BeEquivalentTo(entities);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_All_From_Exercise(User user, Exercise exercise, List<Result> otherResults, List<Result> randomResults)
        {
            var questions = exercise.Questions;

            var entities = questions.Select(question => new Result
            {
                QuestionId = question.Id,
                AnswerId = question.Answers.First().Id,
                UserId = user.Id
            }).ToList();

            otherResults.ForEach(x => x.UserId = user.Id);

            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.Exercises.AddAsync(exercise);
            await context.Results.AddRangeAsync(entities);
            await context.Results.AddRangeAsync(otherResults);
            await context.Results.AddRangeAsync(randomResults);
            await context.SaveChangesAsync();

            var service = new ResultService(context);

            var result = await service.GetAll(user, exercise);

            result.Should().NotBeNullOrEmpty().And.BeEquivalentTo(entities);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_From_Question(User user, Question question, List<Result> otherResults, List<Result> randomResults)
        {
            var entity = new Result
            {
                QuestionId = question.Id,
                AnswerId = question.Answers.First().Id,
                UserId = user.Id,
            };

            otherResults.ForEach(x => x.UserId = user.Id);

            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.Questions.AddAsync(question);
            await context.Results.AddRangeAsync(entity);
            await context.Results.AddRangeAsync(otherResults);
            await context.Results.AddRangeAsync(randomResults);
            await context.SaveChangesAsync();

            var service = new ResultService(context);

            var result = await service.Get(user, question);

            result.Should().NotBeNull().And.BeEquivalentTo(entity);
        }

        [Theory]
        [AutoMoqData]
        public async Task Register_Answers(User user, List<Question> questions)
        {
            var answers = questions.Select(question => new ResultRequest
            {
                QuestionId = question.Id,
                AnswerId = question.Answers.First().Id
            }).ToList();

            var context = TestSetup.SetupContext();

            await context.Users.AddAsync(user);
            await context.Questions.AddRangeAsync(questions);
            await context.SaveChangesAsync();

            var service = new ResultService(context);

            var result = await service.RegisterAnswers(user, answers);

            result.Should().BeTrue();
            context.Results.AsEnumerable().Should().HaveCount(answers.Count);
        }
    }
}
