using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Helpers.JWT;
using Lern_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services.Database
{
    public interface IConceptService : IDatabaseService<Concept, ConceptRequest>
    {
    }

    public class ConceptService : DatabaseService<Concept, ConceptRequest>, IConceptService
    {
        private readonly IAuthorizationService _authorizationService;

        public ConceptService(LernContext context, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService) : base(context, httpContextAccessor)
        {
            _authorizationService = authorizationService;
        }

        protected override IQueryable<Concept> WithDefaultIncludes(DbSet<Concept> set)
        {
            return base.WithDefaultIncludes(set)
                .Include(concept => concept.Courses)
                .ThenInclude(course => course.Exercises)
                .ThenInclude(exercise => exercise.Questions)
                .ThenInclude(question => question.Answers)
                .Include(concept => concept.Exercises)
                .ThenInclude(exercise => exercise.Questions)
                .ThenInclude(question => question.Answers);
        }

        public override async Task<Concept> Get(Guid id, CancellationToken token = default)
        {
            var entity = await base.Get(id, token);

            var canEdit = await _authorizationService.HasWriteAccess(HttpContextAccessor.HttpContext.GetUser(), entity, token);

            if (canEdit)
                return entity;

            return await DbSet
                .Include(concept => concept.Courses)
                .ThenInclude(course => course.Exercises.Where(exercise => exercise.Questions.Any()))
                .ThenInclude(exercise =>
                    exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .Include(concept => concept.Exercises.Where(exercise => exercise.Questions.Any()))
                .ThenInclude(exercise =>
                    exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .ThenInclude(question => question.Answers)
                .Where(concept => concept.Courses.Any() && concept.Exercises.Any(exercise =>
                        exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid))
                    )
                ).FirstOrDefaultAsync(concept => concept.Id == id, token);
        }
    }
}
