using System;
using System.Collections.Generic;
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
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class StateServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Update_Completion_To_100(Mock<IProgressionService> progressionService, Mock<IResultService> resultService, User user, Subject subject, List<Result> randomResults)
        {
            var progression = new Progression
            {
                Subject = subject,
                SubjectId = subject.Id,
                User = user,
                UserId = user.Id,
                Concept = subject.Modules.First().Concepts.First()
            };

            randomResults.ForEach(x => x.UserId = user.Id);

            var results = subject.Modules.SelectMany(module => module.Concepts.SelectMany(concept => concept.Exercises.SelectMany(exercise => exercise.Questions.Select(question => new Result
                {
                    QuestionId = question.Id,
                    AnswerId = question.Answers.First().Id,
                    UserId = user.Id
                }))))
                .Concat(subject.Modules.SelectMany(module => module.Concepts.SelectMany(concept => concept.Courses.SelectMany(course => course.Exercises.SelectMany(exercise => exercise.Questions.Select(question => new Result
                    {
                        QuestionId = question.Id,
                        AnswerId = question.Answers.First().Id,
                        UserId = user.Id
                    }
                )))))).ToList();

            var context = TestSetup.SetupContext();

            progressionService.Setup(x => x.Exists(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            progressionService.Setup(x => x.Get(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(progression);

            progressionService
                .Setup(x => x.ExecuteTransaction(It.IsAny<Action<DbSet<Progression>>>(), It.IsAny<CancellationToken>()))
                .Callback((Action<DbSet<Progression>> action, CancellationToken _) =>
                {
                    action(context.Progressions);
                    context.SaveChanges();
                }).ReturnsAsync(true);
            
            resultService.Setup(x => x.GetAll(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(results);
            
            await context.Users.AddAsync(user);
            await context.Subjects.AddAsync(subject);
            await context.Results.AddRangeAsync(results);
            await context.Results.AddRangeAsync(randomResults);
            await context.Progressions.AddAsync(progression);
            await context.SaveChangesAsync();

            var service = new StateService(context, progressionService.Object, resultService.Object);

            var result = await service.UpdateCompletion(user, subject.Id);

            result.Should().BeTrue();
            context.Progressions.First().Completed.Should().BeTrue();
            context.Progressions.First().Completion.Should().Be(100);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Update_Completion_To_50(Mock<IResultService> resultService, Mock<IProgressionService> progressionService, User user, Subject subject, List<Result> randomResults)
        {
            var progression = new Progression
            {
                Subject = subject,
                SubjectId = subject.Id,
                User = user,
                UserId = user.Id,
                Concept = subject.Modules.First().Concepts.First()
            };

            randomResults.ForEach(x => x.UserId = user.Id);

            var results = subject.Modules.SelectMany(module => module.Concepts.SelectMany(concept => concept.Exercises.SelectMany(exercise => exercise.Questions.Select(question => new Result
                {
                    QuestionId = question.Id,
                    AnswerId = question.Answers.First().Id,
                    UserId = user.Id
                }))))
                .Concat(subject.Modules.SelectMany(module => module.Concepts.SelectMany(concept => concept.Courses.SelectMany(course => course.Exercises.SelectMany(exercise => exercise.Questions.Select(question => new Result
                    {
                        QuestionId = question.Id,
                        AnswerId = question.Answers.First().Id,
                        UserId = user.Id
                    }
                )))))).ToList();

            results.RemoveRange(0, results.Count / 2);

            var context = TestSetup.SetupContext();

            progressionService.Setup(x => x.Exists(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            progressionService.Setup(x => x.Get(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(progression);

            progressionService
                .Setup(x => x.ExecuteTransaction(It.IsAny<Action<DbSet<Progression>>>(), It.IsAny<CancellationToken>()))
                .Callback((Action<DbSet<Progression>> action, CancellationToken _) =>
                {
                    action(context.Progressions);
                    context.SaveChanges();
                }).ReturnsAsync(true);
            
            resultService.Setup(x => x.GetAll(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(results);
            
            await context.Users.AddAsync(user);
            await context.Subjects.AddAsync(subject);
            await context.Results.AddRangeAsync(results);
            await context.Results.AddRangeAsync(randomResults);
            await context.Progressions.AddAsync(progression);
            await context.SaveChangesAsync();

            var service = new StateService(context, progressionService.Object, resultService.Object);

            var result = await service.UpdateCompletion(user, subject.Id);

            result.Should().BeTrue();
            context.Progressions.First().Completed.Should().BeFalse();
            context.Progressions.First().Completion.Should().BeApproximately(50, 1);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Update_Completion_To_0(Mock<IResultService> resultService, Mock<IProgressionService> progressionService, User user, Subject subject, List<Result> randomResults)
        {
            var progression = new Progression
            {
                Subject = subject,
                SubjectId = subject.Id,
                User = user,
                UserId = user.Id,
                Concept = subject.Modules.First().Concepts.First()
            };

            randomResults.ForEach(x => x.UserId = user.Id);

            var context = TestSetup.SetupContext();

            progressionService.Setup(x => x.Exists(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            progressionService.Setup(x => x.Get(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(progression);

            progressionService
                .Setup(x => x.ExecuteTransaction(It.IsAny<Action<DbSet<Progression>>>(), It.IsAny<CancellationToken>()))
                .Callback((Action<DbSet<Progression>> action, CancellationToken _) =>
                {
                    action(context.Progressions);
                    context.SaveChanges();
                }).ReturnsAsync(true);
            
            resultService.Setup(x => x.GetAll(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Result>());
            
            await context.Users.AddAsync(user);
            await context.Subjects.AddAsync(subject);
            await context.Results.AddRangeAsync(randomResults);
            await context.Progressions.AddAsync(progression);
            await context.SaveChangesAsync();

            var service = new StateService(context, progressionService.Object, resultService.Object);

            var result = await service.UpdateCompletion(user, subject.Id);

            result.Should().BeTrue();
            context.Progressions.First().Completed.Should().BeFalse();
            context.Progressions.First().Completion.Should().Be(0);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Update_Score_To_100(Mock<IResultService> resultService, Mock<IProgressionService> progressionService, User user, Subject subject, List<Result> randomResults)
        {
            var progression = new Progression
            {
                Subject = subject,
                SubjectId = subject.Id,
                User = user,
                UserId = user.Id,
                Concept = subject.Modules.First().Concepts.First()
            };

            randomResults.ForEach(x => x.UserId = user.Id);

            var results = subject.Modules.SelectMany(module => module.Concepts.SelectMany(concept => concept.Exercises.SelectMany(exercise => exercise.Questions.Select(question => new Result
                {
                    QuestionId = question.Id,
                    AnswerId = question.Answers.First(answer => answer.Valid).Id,
                    UserId = user.Id
                }))))
                .Concat(subject.Modules.SelectMany(module => module.Concepts.SelectMany(concept => concept.Courses.SelectMany(course => course.Exercises.SelectMany(exercise => exercise.Questions.Select(question => new Result
                    {
                        QuestionId = question.Id,
                        AnswerId = question.Answers.First(answer => answer.Valid).Id,
                        UserId = user.Id
                    }
                )))))).ToList();

            var context = TestSetup.SetupContext();

            progressionService.Setup(x => x.Exists(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            progressionService.Setup(x => x.Get(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(progression);

            progressionService
                .Setup(x => x.ExecuteTransaction(It.IsAny<Action<DbSet<Progression>>>(), It.IsAny<CancellationToken>()))
                .Callback((Action<DbSet<Progression>> action, CancellationToken _) =>
                {
                    action(context.Progressions);
                    context.SaveChanges();
                }).ReturnsAsync(true);
            
            resultService.Setup(x => x.GetAll(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(results);
            
            await context.Users.AddAsync(user);
            await context.Subjects.AddAsync(subject);
            await context.Results.AddRangeAsync(results);
            await context.Results.AddRangeAsync(randomResults);
            await context.Progressions.AddAsync(progression);
            await context.SaveChangesAsync();

            var service = new StateService(context, progressionService.Object, resultService.Object);

            var result = await service.UpdateCompletion(user, subject.Id);

            result.Should().BeTrue();
            context.Progressions.First().Completed.Should().BeTrue();
            context.Progressions.First().Score.Should().Be(100);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Update_Score_To_50(Mock<IResultService> resultService, Mock<IProgressionService> progressionService, User user, Subject subject, List<Result> randomResults)
        {
            var progression = new Progression
            {
                Subject = subject,
                SubjectId = subject.Id,
                User = user,
                UserId = user.Id,
                Concept = subject.Modules.First().Concepts.First()
            };

            randomResults.ForEach(x => x.UserId = user.Id);

            var results = subject.Modules.SelectMany(module => module.Concepts.SelectMany(concept => concept.Exercises.SelectMany(exercise => exercise.Questions.Select(question => new Result
                {
                    QuestionId = question.Id,
                    Question = question,
                    AnswerId = question.Answers.First(answer => answer.Valid).Id,
                    UserId = user.Id
                }))))
                .Concat(subject.Modules.SelectMany(module => module.Concepts.SelectMany(concept => concept.Courses.SelectMany(course => course.Exercises.SelectMany(exercise => exercise.Questions.Select(question => new Result
                    {
                        QuestionId = question.Id,
                        Question = question,
                        AnswerId = question.Answers.First(answer => answer.Valid).Id,
                        UserId = user.Id
                    }
                )))))).ToList();

            var resultsCount = results.Count;

            for (var i = 0; i < resultsCount / 2; i++)
            {
                var currentResult = results.ElementAt(i);
                currentResult.AnswerId = currentResult.Question.Answers.First(answer => !answer.Valid).Id;
            }

            var context = TestSetup.SetupContext();

            progressionService.Setup(x => x.Exists(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            progressionService.Setup(x => x.Get(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(progression);

            progressionService
                .Setup(x => x.ExecuteTransaction(It.IsAny<Action<DbSet<Progression>>>(), It.IsAny<CancellationToken>()))
                .Callback((Action<DbSet<Progression>> action, CancellationToken _) =>
                {
                    action(context.Progressions);
                    context.SaveChanges();
                }).ReturnsAsync(true);
            
            resultService.Setup(x => x.GetAll(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(results);
            
            await context.Users.AddAsync(user);
            await context.Subjects.AddAsync(subject);
            await context.Results.AddRangeAsync(results);
            await context.Results.AddRangeAsync(randomResults);
            await context.Progressions.AddAsync(progression);
            await context.SaveChangesAsync();

            var service = new StateService(context, progressionService.Object, resultService.Object);

            var result = await service.UpdateCompletion(user, subject.Id);

            result.Should().BeTrue();
            context.Progressions.First().Completed.Should().BeTrue();
            context.Progressions.First().Score.Should().BeApproximately(50, 1);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Update_Score_To_0(Mock<IResultService> resultService, Mock<IProgressionService> progressionService, User user, Subject subject, List<Result> randomResults)
        {
            var progression = new Progression
            {
                Subject = subject,
                SubjectId = subject.Id,
                User = user,
                UserId = user.Id,
                Concept = subject.Modules.First().Concepts.First()
            };

            randomResults.ForEach(x => x.UserId = user.Id);

            var results = subject.Modules.SelectMany(module => module.Concepts.SelectMany(concept => concept.Exercises.SelectMany(exercise => exercise.Questions.Select(question => new Result
                {
                    QuestionId = question.Id,
                    AnswerId = question.Answers.First(answer => !answer.Valid).Id,
                    UserId = user.Id
                }))))
                .Concat(subject.Modules.SelectMany(module => module.Concepts.SelectMany(concept => concept.Courses.SelectMany(course => course.Exercises.SelectMany(exercise => exercise.Questions.Select(question => new Result
                    {
                        QuestionId = question.Id,
                        AnswerId = question.Answers.First(answer => !answer.Valid).Id,
                        UserId = user.Id
                    }
                )))))).ToList();

            var context = TestSetup.SetupContext();

            progressionService.Setup(x => x.Exists(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            progressionService.Setup(x => x.Get(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(progression);

            progressionService
                .Setup(x => x.ExecuteTransaction(It.IsAny<Action<DbSet<Progression>>>(), It.IsAny<CancellationToken>()))
                .Callback((Action<DbSet<Progression>> action, CancellationToken _) =>
                {
                    action(context.Progressions);
                    context.SaveChanges();
                }).ReturnsAsync(true);
            
            resultService.Setup(x => x.GetAll(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(results);
            
            await context.Users.AddAsync(user);
            await context.Subjects.AddAsync(subject);
            await context.Results.AddRangeAsync(results);
            await context.Results.AddRangeAsync(randomResults);
            await context.Progressions.AddAsync(progression);
            await context.SaveChangesAsync();

            var service = new StateService(context, progressionService.Object, resultService.Object);

            var result = await service.UpdateCompletion(user, subject.Id);

            result.Should().BeTrue();
            context.Progressions.First().Completed.Should().BeTrue();
            context.Progressions.First().Score.Should().Be(0);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Register_Answers(IProgressionService progressionService, Mock<IResultService> resultService, User user, List<Question> questions)
        {
            var answers = questions.Select(question => new ResultRequest
            {
                QuestionId = question.Id,
                AnswerId = question.Answers.First().Id
            }).ToList();

            var context = TestSetup.SetupContext();

            resultService
                .Setup(x => x.ExecuteTransaction(It.IsAny<Func<DbSet<Result>, Task>>(), It.IsAny<CancellationToken>()))
                .Callback((Func<DbSet<Result>, Task> action, CancellationToken _) =>
                {
                    action(context.Results);
                    context.SaveChanges();
                }).ReturnsAsync(true);

            await context.Users.AddAsync(user);
            await context.Questions.AddRangeAsync(questions);
            await context.SaveChangesAsync();

            var service = new StateService(context, progressionService, resultService.Object);

            var result = await service.RegisterAnswers(user, answers);

            result.Should().BeTrue();
            context.Results.AsEnumerable().Should().HaveCount(answers.Count);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State(IProgressionService progressionService, IResultService resultService, Subject subject, Subject invalidSubject)
        {
            subject.Modules.First().Concepts.First().Exercises.First().Questions.First().Answers.First().Valid = true;

            invalidSubject.Modules.First().Concepts.First().Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);
            invalidSubject.Modules.First().Concepts.First().Courses.Clear();

            var context = TestSetup.SetupContext();

            await context.Subjects.AddAsync(subject);
            await context.Subjects.AddAsync(invalidSubject);
            await context.SaveChangesAsync();

            var service = new StateService(context, progressionService, resultService);
            var result = await service.UpdateSubjectState(subject.Id);
            var invalidResult = await service.UpdateSubjectState(invalidSubject.Id);

            result.State.Should().Be(SubjectState.Approved);
            invalidResult.State.Should().Be(SubjectState.Invalid);
        }
    }
}
