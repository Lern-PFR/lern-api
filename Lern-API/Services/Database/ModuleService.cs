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
    public interface IModuleService : IDatabaseService<Module, ModuleRequest>
    {
    }

    public class ModuleService : DatabaseService<Module, ModuleRequest>, IModuleService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IStateService _stateService;

        public ModuleService(LernContext context, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService, IStateService stateService) : base(context, httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _stateService = stateService;
        }

        protected override IQueryable<Module> WithDefaultIncludes(DbSet<Module> set)
        {
            return base.WithDefaultIncludes(set)
                .Include(module => module.Concepts)
                .ThenInclude(concept => concept.Lessons)
                .ThenInclude(lesson => lesson.Exercises)
                .ThenInclude(exercise => exercise.Questions)
                .ThenInclude(question => question.Answers)
                .Include(module => module.Concepts)
                .ThenInclude(concept => concept.Exercises)
                .ThenInclude(exercise => exercise.Questions)
                .ThenInclude(question => question.Answers);
        }

        public override async Task<Module> Get(Guid id, CancellationToken token = default)
        {
            var entity = await base.Get(id, token);
            
            if (entity == null)
                return null;

            var canEdit = await _authorizationService.HasWriteAccess(HttpContextAccessor.HttpContext.GetUser(), entity, token);

            if (canEdit)
                return entity;

            return await DbSet
                .Include(module =>
                    module.Concepts.Where(concept => concept.Lessons.Any() && concept.Exercises.Any(exercise => exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid)))))
                .ThenInclude(concept => concept.Lessons)
                .ThenInclude(lesson => lesson.Exercises.Where(exercise => exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid))))
                .ThenInclude(exercise =>
                    exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .ThenInclude(question => question.Answers)
                .Include(module =>
                    module.Concepts.Where(concept => concept.Lessons.Any() && concept.Exercises.Any(exercise => exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid)))))
                .ThenInclude(concept => concept.Exercises.Where(exercise => exercise.Questions.Any()))
                .ThenInclude(exercise =>
                    exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .ThenInclude(question => question.Answers)
                .Where(module =>
                    module.Concepts.Any() && module.Concepts.All(concept => concept.Lessons.Any() /*&& concept.Exercises.Any() && concept.Exercises.All(exercise =>
                        exercise.Questions.Any() && exercise.Questions.All(question => question.Answers.Any(answer => answer.Valid))*/
                    )
                ).FirstOrDefaultAsync(module => module.Id == id, token);
        }

        public override async Task<Module> Create(ModuleRequest entity, CancellationToken token = default)
        {
            var result = await base.Create(entity, token);

            if (result == null)
                return null;

            await _stateService.UpdateSubjectState(result.SubjectId, token);

            return result;
        }

        public override async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            var entity = await base.Get(id, token);
            var result = await base.Delete(id, token);

            if (!result)
                return false;

            await _stateService.UpdateSubjectState(entity.SubjectId, token);

            return true;
        }

        public override async Task<Module> Update(Guid id, ModuleRequest entity, CancellationToken token = default)
        {
            var result = await base.Update(id, entity, token);

            if (result == null)
                return null;

            await _stateService.UpdateSubjectState(result.SubjectId, token);

            return result;
        }
    }
}
