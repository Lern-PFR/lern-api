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
        private readonly ISubjectService _subjectService;

        public ConceptService(LernContext context, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService, ISubjectService subjectService) : base(context, httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _subjectService = subjectService;
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
            
            if (entity == null)
                return null;

            var canEdit = await _authorizationService.HasWriteAccess(HttpContextAccessor.HttpContext.GetUser(), entity, token);

            if (canEdit)
                return entity;

            return await DbSet
                .Include(concept => concept.Courses)
                .ThenInclude(course => course.Exercises.Where(exercise => exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid))))
                .ThenInclude(exercise =>
                    exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .ThenInclude(question => question.Answers)
                .Include(concept => concept.Exercises.Where(exercise => exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid))))
                .ThenInclude(exercise =>
                    exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .ThenInclude(question => question.Answers)
                .Where(concept => concept.Courses.Any() && concept.Exercises.Any() && concept.Exercises.All(exercise =>
                        exercise.Questions.Any() && exercise.Questions.All(question => question.Answers.Any(answer => answer.Valid))
                    )
                ).FirstOrDefaultAsync(concept => concept.Id == id, token);
        }

        public override async Task<Concept> Create(ConceptRequest entity, CancellationToken token = default)
        {
            var result = await base.Create(entity, token);

            if (result == null)
                return null;

            var subject = await Context.Subjects.FirstOrDefaultAsync(x => x.Modules.Any(module => module.Id == result.ModuleId), token);
            await _subjectService.UpdateState(subject?.Id ?? default, token);

            return result;
        }

        public override async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            var entity = await base.Get(id, token);
            var result = await base.Delete(id, token);

            if (!result)
                return false;

            var subject = await Context.Subjects.FirstOrDefaultAsync(x => x.Modules.Any(module => module.Id == entity.ModuleId), token);
            await _subjectService.UpdateState(subject?.Id ?? default, token);

            return true;
        }

        public override async Task<Concept> Update(Guid id, ConceptRequest entity, CancellationToken token = default)
        {
            var result = await base.Update(id, entity, token);

            if (result == null)
                return null;

            var subject = await Context.Subjects.FirstOrDefaultAsync(x => x.Modules.Any(module => module.Id == result.ModuleId), token);
            await _subjectService.UpdateState(subject?.Id ?? default, token);

            return result;
        }
    }
}
