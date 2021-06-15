using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.Controllers;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.DataTransferObjects.Responses;
using Lern_API.Models;
using Lern_API.Services.Database;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Lern_API.Tests.Controllers
{
    public class ProgressionControllerShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Return_All_Progressions(Mock<IProgressionService> service, ISubjectService subjects, IConceptService concepts, List<Progression> progressions, User user)
        {
            service.Setup(x => x.GetAll(user, It.IsAny<CancellationToken>())).ReturnsAsync(progressions);

            var controller = TestSetup.SetupController<ProgressionController>(service.Object, subjects, concepts).SetupSession(user);

            var result = await controller.GetProgressions();
            
            result.Should().NotBeNull().And.BeEquivalentTo(progressions.Select(x => new ProgressionResponse(x)));
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Progression_Or_One_Of_204_404(Mock<IProgressionService> service, Mock<ISubjectService> subjects, IConceptService concepts, Progression progression, User user, Subject subject, Subject badSubject)
        {
            subjects.Setup(x => x.Get(subject.Id, It.IsAny<CancellationToken>())).ReturnsAsync(subject);
            subjects.Setup(x => x.Get(badSubject.Id, It.IsAny<CancellationToken>())).ReturnsAsync(badSubject);
            subjects.Setup(x => x.Get(Guid.Empty, It.IsAny<CancellationToken>())).ReturnsAsync((Subject) null);

            service.Setup(x => x.Get(user, subject, It.IsAny<CancellationToken>())).ReturnsAsync(progression);
            service.Setup(x => x.Get(user, badSubject, It.IsAny<CancellationToken>())).ReturnsAsync((Progression) null);

            var controller = TestSetup.SetupController<ProgressionController>(service.Object, subjects.Object, concepts).SetupSession(user);

            var result = await controller.GetProgression(subject.Id);
            var result204 = await controller.GetProgression(badSubject.Id);
            var result404 = await controller.GetProgression(Guid.Empty);

            result.Value.Should().NotBeNull().And.BeEquivalentTo(new ProgressionResponse(progression));
            
            result204.Value.Should().BeNull();
            result204.Result.Should().BeOfType(typeof(NoContentResult));

            result404.Value.Should().BeNull();
            result404.Result.Should().BeOfType(typeof(NotFoundResult));
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Progression_Or_403(Mock<IProgressionService> service, Mock<ISubjectService> subjects, Mock<IConceptService> concepts, User user, Subject subject, Subject invalidSubject, Concept concept)
        {
            concepts.Setup(x => x.Get(concept.Id, It.IsAny<CancellationToken>())).ReturnsAsync(concept);
            subjects.Setup(x => x.Get(subject.Id, It.IsAny<CancellationToken>())).ReturnsAsync(subject);
            subjects.Setup(x => x.Get(invalidSubject.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invalidSubject);

            service.Setup(x => x.Update(user, subject, concept, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            service.Setup(x => x.Update(user, invalidSubject, concept, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var controller = TestSetup.SetupController<ProgressionController>(service.Object, subjects.Object, concepts.Object).SetupSession(user);

            var validRequest = new ProgressionRequest
            {
                ConceptId = concept.Id,
                SubjectId = subject.Id,
            };

            var invalidRequest = new ProgressionRequest
            {
                ConceptId = concept.Id,
                SubjectId = invalidSubject.Id,
            };

            var result = await controller.UpdateProgression(validRequest);
            var result403 = await controller.UpdateProgression(invalidRequest);

            result.Should().NotBeNull().And.BeOfType<OkResult>();
            result403.Should().NotBeNull().And.BeOfType<ForbidResult>();
        }
    }
}
