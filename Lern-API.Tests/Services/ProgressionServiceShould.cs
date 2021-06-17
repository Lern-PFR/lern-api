using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.Models;
using Lern_API.Services.Database;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Moq;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class ProgressionServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_All_Progressions_By_User(User user, List<Progression> validProgressions,
            List<Progression> invalidProgressions)
        {
            var context = TestSetup.SetupContext();
            
            foreach (var progression in validProgressions)
            {
                progression.User = user;
                progression.UserId = user.Id;
            }

            await context.Users.AddAsync(user);
            await context.Progressions.AddRangeAsync(invalidProgressions);
            await context.Progressions.AddRangeAsync(validProgressions);
            await context.SaveChangesAsync();

            var service = new ProgressionService(context);
            var result = await service.GetAll(user);

            result.Should().NotBeNull().And.BeEquivalentTo(validProgressions);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Single_Progression(User user, Subject subject, Concept concept)
        {
            var context = TestSetup.SetupContext();

            var progression = new Progression
            {
                UserId = user.Id,
                User = user,
                SubjectId = subject.Id,
                Subject = subject,
                ConceptId = concept.Id,
                Concept = concept
            };

            await context.Users.AddAsync(user);
            await context.Subjects.AddAsync(subject);
            await context.Concepts.AddAsync(concept);
            await context.Progressions.AddAsync(progression);
            await context.SaveChangesAsync();

            var service = new ProgressionService(context);
            var result = await service.Get(user, subject);

            result.Should().NotBeNull().And.BeEquivalentTo(progression);
        }

        [Theory]
        [AutoMoqData]
        public async Task Create_Progression(User user, Subject subject, Concept concept)
        {
            var context = TestSetup.SetupContext();

            var progression = new Progression
            {
                UserId = user.Id,
                User = user,
                SubjectId = subject.Id,
                Subject = subject,
                ConceptId = concept.Id,
                Concept = concept
            };

            await context.Users.AddAsync(user);
            await context.Subjects.AddAsync(subject);
            await context.Concepts.AddAsync(concept);
            await context.SaveChangesAsync();

            var service = new ProgressionService(context);
            var result = await service.Create(user, subject, concept);

            var storedResult = context.Progressions.FirstOrDefault();

            result.Should().BeTrue();
            storedResult.Should().NotBeNull().And.BeEquivalentTo(progression, TestSetup.IgnoreTimestamps<Progression>());
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Progression(User user, Subject subject, Concept concept, Concept newConcept)
        {
            var context = TestSetup.SetupContext();

            var progression = new Progression
            {
                UserId = user.Id,
                User = user,
                SubjectId = subject.Id,
                Subject = subject,
                ConceptId = concept.Id,
                Concept = concept
            };

            var expected = new Progression
            {
                UserId = user.Id,
                User = user,
                SubjectId = subject.Id,
                Subject = subject,
                ConceptId = newConcept.Id,
                Concept = newConcept
            };

            await context.Users.AddAsync(user);
            await context.Subjects.AddAsync(subject);
            await context.Concepts.AddRangeAsync(concept, newConcept);
            await context.Progressions.AddAsync(progression);
            await context.SaveChangesAsync();

            var service = new ProgressionService(context);
            var result = await service.Update(user, subject, newConcept);

            var storedResult = context.Progressions.FirstOrDefault();

            result.Should().BeTrue();
            storedResult.Should().NotBeNull().And.BeEquivalentTo(expected, TestSetup.IgnoreTimestamps<Progression>());
        }

        [Theory]
        [AutoMoqData]
        public async Task Check_If_Progression_Exists(User user, User badUser, Subject subject, Concept concept)
        {
            var context = TestSetup.SetupContext();

            var progression = new Progression
            {
                UserId = user.Id,
                User = user,
                SubjectId = subject.Id,
                Subject = subject,
                ConceptId = concept.Id,
                Concept = concept
            };

            await context.Users.AddRangeAsync(user, badUser);
            await context.Subjects.AddAsync(subject);
            await context.Concepts.AddAsync(concept);
            await context.Progressions.AddAsync(progression);
            await context.SaveChangesAsync();

            var service = new ProgressionService(context);

            var result = await service.Exists(user, subject);
            var badResult = await service.Exists(badUser, subject);

            result.Should().BeTrue();
            badResult.Should().BeFalse();
        }
    }
}
