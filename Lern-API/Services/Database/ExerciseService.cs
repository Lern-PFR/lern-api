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
    public interface IExerciseService : IDatabaseService<Exercise, ExerciseRequest>
    {
    }

    public class ExerciseService : DatabaseService<Exercise, ExerciseRequest>, IExerciseService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IStateService _stateService;

        public ExerciseService(LernContext context, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService, IStateService stateService) : base(context, httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _stateService = stateService;
        }

        protected override IQueryable<Exercise> WithDefaultIncludes(DbSet<Exercise> set)
        {
            return base.WithDefaultIncludes(set)
                .Include(exercise => exercise.Questions)
                .ThenInclude(question => question.Answers);
        }

        public override async Task<Exercise> Get(Guid id, CancellationToken token = default)
        {
            var entity = await base.Get(id, token);
            
            if (entity == null)
                return null;

            var canEdit = await _authorizationService.HasWriteAccess(HttpContextAccessor.HttpContext.GetUser(), entity, token);

            if (canEdit)
                return entity;

            return await DbSet
                .Include(exercise =>
                    exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .ThenInclude(question => question.Answers)
                .Where(exercise => exercise.Questions.Any() && exercise.Questions.All(question => question.Answers.Any(answer => answer.Valid)))
                .FirstOrDefaultAsync(exercise => exercise.Id == id, token);
        }

        public override async Task<Exercise> Create(ExerciseRequest entity, CancellationToken token = default)
        {
            var result = await base.Create(entity, token);

            if (result == null)
                return null;

            var subject = await Context.Subjects.FirstOrDefaultAsync(x => x.Modules.Any(module => module.Concepts.Any(concept => concept.Id == result.ConceptId) || module.Concepts.Any(concept => concept.Courses.Any(course => course.Id == result.CourseId))), token);
            await _stateService.UpdateSubjectState(subject?.Id ?? default, token);

            return result;
        }

        public override async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            var entity = await base.Get(id, token);
            var result = await base.Delete(id, token);

            if (!result)
                return false;

            var subject = await Context.Subjects.FirstOrDefaultAsync(x => x.Modules.Any(module => module.Concepts.Any(concept => concept.Id == entity.ConceptId) || module.Concepts.Any(concept => concept.Courses.Any(course => course.Id == entity.CourseId))), token);
            await _stateService.UpdateSubjectState(subject?.Id ?? default, token);

            return true;
        }

        public override async Task<Exercise> Update(Guid id, ExerciseRequest entity, CancellationToken token = default)
        {
            var result = await base.Update(id, entity, token);

            if (result == null)
                return null;

            var subject = await Context.Subjects.FirstOrDefaultAsync(x => x.Modules.Any(module => module.Concepts.Any(concept => concept.Id == result.ConceptId) || module.Concepts.Any(concept => concept.Courses.Any(course => course.Id == result.CourseId))), token);
            await _stateService.UpdateSubjectState(subject?.Id ?? default, token);

            return result;
        }
    }
}
