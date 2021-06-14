using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Helpers.Models;
using Lern_API.Models;
using Lern_API.Services.Database;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Moq;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class QuestionServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Create_Answers(ISubjectService subjectService, QuestionRequest request)
        {
            var context = TestSetup.SetupContext();
            var service = new QuestionService(context, TestSetup.SetupHttpContext(), subjectService);

            var result = await service.Create(request);

            result.Answers.Should().NotBeNull().And.BeEquivalentTo(request.Answers);
            context.Answers.AsEnumerable().Should().BeEquivalentTo(result.Answers);
        }

        [Theory]
        [AutoMoqData]
        public async Task Delete_Answers(ISubjectService subjectService, Question entity, QuestionRequest request)
        {
            var context = TestSetup.SetupContext();
            var service = new QuestionService(context, TestSetup.SetupHttpContext(), subjectService);

            await context.Questions.AddAsync(entity);
            await context.SaveChangesAsync();

            request.Answers.Clear();
            var result = await service.Update(entity.Id, request);

            result.Answers.Should().BeNullOrEmpty();
            context.Answers.AsEnumerable().Should().BeNullOrEmpty();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Answers(ISubjectService subjectService, Question entity, QuestionRequest request, Answer answer, AnswerRequest answerRequest)
        {
            var context = TestSetup.SetupContext();
            var service = new QuestionService(context, TestSetup.SetupHttpContext(), subjectService);
            
            answer.QuestionId = entity.Id;

            var newAnswer = new Answer();
            newAnswer.CloneFrom(answer);
            newAnswer.CloneFrom(answerRequest);

            request.Answers.Clear();
            request.Answers.Add(answerRequest);

            entity.Answers.Clear();
            entity.Answers.Add(answer);
            await context.Questions.AddAsync(entity);
            await context.SaveChangesAsync();
            
            var result = await service.Update(entity.Id, request);

            result.Answers.Should().HaveCount(1);
            result.Answers.First().Should().BeEquivalentTo(answerRequest);
            context.Answers.AsEnumerable().Should().HaveCount(1).And.ContainEquivalentOf(result.Answers.First());
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Update(Mock<ISubjectService> subjectService, Question question, QuestionRequest request)
        {
            subjectService.Setup(x => x.UpdateState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();
            
            await context.Questions.AddAsync(question);
            await context.SaveChangesAsync();

            var service = new QuestionService(context, httpContext, subjectService.Object);
            await service.Update(question.Id, request);

            subjectService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Create(Mock<ISubjectService> subjectService, QuestionRequest request)
        {
            subjectService.Setup(x => x.UpdateState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();
            
            var service = new QuestionService(context, httpContext, subjectService.Object);
            await service.Create(request);

            subjectService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Delete(Mock<ISubjectService> subjectService, Question question)
        {
            subjectService.Setup(x => x.UpdateState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();

            await context.Questions.AddAsync(question);
            await context.SaveChangesAsync();

            var service = new QuestionService(context, httpContext, subjectService.Object);
            await service.Delete(question.Id);

            subjectService.VerifyAll();
        }
    }
}
