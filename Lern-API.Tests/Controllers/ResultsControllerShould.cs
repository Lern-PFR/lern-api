using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.Controllers;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Services.Database;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Lern_API.Tests.Controllers
{
    public class ResultsControllerShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_Result_From_Question(Mock<IResultService> resultService, Mock<IQuestionService> questionService, IExerciseService exerciseService, IStateService stateService, User user, Question question, Result entity)
        {
            resultService.Setup(x => x.Get(user, question, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            questionService.Setup(x => x.Get(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);

            var controller = TestSetup.SetupController<ResultsController>(resultService.Object, questionService.Object, exerciseService, stateService).SetupSession(user);

            var result = await controller.GetFromQuestion(question.Id);

            result.Should().NotBeNull();
            result.Value.Should().NotBeNull().And.BeEquivalentTo(entity);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_204_From_Question(Mock<IResultService> resultService, Mock<IQuestionService> questionService, IExerciseService exerciseService, IStateService stateService, User user, Question question)
        {
            resultService.Setup(x => x.Get(user, question, It.IsAny<CancellationToken>())).ReturnsAsync((Result) null);
            questionService.Setup(x => x.Get(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);

            var controller = TestSetup.SetupController<ResultsController>(resultService.Object, questionService.Object, exerciseService, stateService).SetupSession(user);

            var result = await controller.GetFromQuestion(question.Id);

            result.Should().NotBeNull();
            result.Result.Should().BeOfType<NoContentResult>();
            result.Value.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_404_From_Question(Mock<IResultService> resultService, Mock<IQuestionService> questionService, IExerciseService exerciseService, IStateService stateService, User user, Question question, Result entity)
        {
            resultService.Setup(x => x.Get(user, question, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            questionService.Setup(x => x.Get(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Question) null);

            var controller = TestSetup.SetupController<ResultsController>(resultService.Object, questionService.Object, exerciseService, stateService).SetupSession(user);

            var result = await controller.GetFromQuestion(question.Id);

            result.Should().NotBeNull();
            result.Result.Should().BeOfType<NotFoundResult>();
            result.Value.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Results_From_Exercise(Mock<IResultService> resultService, IQuestionService questionService, Mock<IExerciseService> exerciseService, IStateService stateService, User user, Exercise exercise, List<Result> entities)
        {
            resultService.Setup(x => x.GetAll(user, exercise, It.IsAny<CancellationToken>())).ReturnsAsync(entities);
            exerciseService.Setup(x => x.Get(exercise.Id, It.IsAny<CancellationToken>())).ReturnsAsync(exercise);

            var controller = TestSetup.SetupController<ResultsController>(resultService.Object, questionService, exerciseService.Object, stateService).SetupSession(user);

            var result = await controller.GetFromExercise(exercise.Id);

            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult) result.Result).Value.Should().NotBeNull().And.BeEquivalentTo(entities);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_204_From_Exercise(Mock<IResultService> resultService, IQuestionService questionService, Mock<IExerciseService> exerciseService, IStateService stateService, User user, Exercise exercise)
        {
            resultService.Setup(x => x.GetAll(user, exercise, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Result>());
            exerciseService.Setup(x => x.Get(exercise.Id, It.IsAny<CancellationToken>())).ReturnsAsync(exercise);

            var controller = TestSetup.SetupController<ResultsController>(resultService.Object, questionService, exerciseService.Object, stateService).SetupSession(user);

            var result = await controller.GetFromExercise(exercise.Id);

            result.Should().NotBeNull();
            result.Result.Should().BeOfType<NoContentResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_404_From_Exercise(Mock<IResultService> resultService, IQuestionService questionService, Mock<IExerciseService> exerciseService, IStateService stateService, User user, Exercise exercise, List<Result> entities)
        {
            resultService.Setup(x => x.GetAll(user, exercise, It.IsAny<CancellationToken>())).ReturnsAsync(entities);
            exerciseService.Setup(x => x.Get(exercise.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Exercise) null);

            var controller = TestSetup.SetupController<ResultsController>(resultService.Object, questionService, exerciseService.Object, stateService).SetupSession(user);

            var result = await controller.GetFromExercise(exercise.Id);

            result.Should().NotBeNull();
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Register_Answers(IResultService resultService, IQuestionService questionService, IExerciseService exerciseService, Mock<IStateService> stateService, User user, List<ResultRequest> entities)
        {
            stateService.Setup(x => x.RegisterAnswers(user, entities, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var controller = TestSetup.SetupController<ResultsController>(resultService, questionService, exerciseService, stateService.Object).SetupSession(user);

            var result = await controller.RegisterAnswers(entities);

            stateService.VerifyAll();
            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();
        }

        [Theory]
        [AutoMoqData]
        public async Task Not_Register_Answers_And_403(Mock<IResultService> resultService, IQuestionService questionService, IExerciseService exerciseService, Mock<IStateService> stateService, User user, List<ResultRequest> entities)
        {
            stateService.Setup(x => x.RegisterAnswers(user, entities, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var controller = TestSetup.SetupController<ResultsController>(resultService.Object, questionService, exerciseService, stateService.Object).SetupSession(user);

            var result = await controller.RegisterAnswers(entities);

            stateService.VerifyAll();
            result.Should().NotBeNull();
            result.Should().BeOfType<ForbidResult>();
        }
    }
}
