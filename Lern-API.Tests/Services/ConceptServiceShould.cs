using System;
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
using Moq;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class ConceptServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_Entire_Concept_With_Write_Access(Mock<IAuthorizationService> authorizationService, ISubjectService subjectService, Concept concept, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, concept, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            concept.Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Users.AddAsync(user);
            await context.Concepts.AddRangeAsync(concept);
            await context.SaveChangesAsync();

            var service = new ConceptService(context, httpContext, authorizationService.Object, subjectService);
            var result = await service.Get(concept.Id);

            result.Should().BeEquivalentTo(concept);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Concept_With_A_Course_And_An_Exercise(Mock<IAuthorizationService> authorizationService, ISubjectService subjectService, Concept concept, Concept invalidConcept, User user)
        {
            authorizationService.Setup(x => x.HasWriteAccess(user, It.IsAny<Concept>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            concept.Exercises.First().Questions.First().Answers.First().Valid = true;

            invalidConcept.Exercises.First().Questions.First().Answers.ForEach(x => x.Valid = false);
            invalidConcept.Courses.Clear();

            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext().SetupSession(user);

            await context.Concepts.AddAsync(concept);
            await context.Concepts.AddAsync(invalidConcept);
            await context.SaveChangesAsync();

            var service = new ConceptService(context, httpContext, authorizationService.Object, subjectService);
            var result = await service.Get(concept.Id);
            var invalidResult = await service.Get(invalidConcept.Id);

            result.Should().NotBeNull().And.BeEquivalentTo(concept);
            invalidResult.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Update(Mock<IAuthorizationService> authorizationService, Mock<ISubjectService> subjectService, Concept concept, ConceptRequest request)
        {
            subjectService.Setup(x => x.UpdateState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();
            
            await context.Concepts.AddAsync(concept);
            await context.SaveChangesAsync();

            var service = new ConceptService(context, httpContext, authorizationService.Object, subjectService.Object);
            await service.Update(concept.Id, request);

            subjectService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Create(Mock<IAuthorizationService> authorizationService, Mock<ISubjectService> subjectService, ConceptRequest request)
        {
            subjectService.Setup(x => x.UpdateState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();
            
            var service = new ConceptService(context, httpContext, authorizationService.Object, subjectService.Object);
            await service.Create(request);

            subjectService.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Subject_State_On_Delete(Mock<IAuthorizationService> authorizationService, Mock<ISubjectService> subjectService, Concept concept)
        {
            subjectService.Setup(x => x.UpdateState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
            
            var context = TestSetup.SetupContext();
            var httpContext = TestSetup.SetupHttpContext();

            await context.Concepts.AddAsync(concept);
            await context.SaveChangesAsync();

            var service = new ConceptService(context, httpContext, authorizationService.Object, subjectService.Object);
            await service.Delete(concept.Id);

            subjectService.VerifyAll();
        }
    }
}
