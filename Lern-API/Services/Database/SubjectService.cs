using System;
using System.Collections.Generic;
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
    public interface ISubjectService : IDatabaseService<Subject, SubjectRequest>
    {
        Task<IEnumerable<Subject>> GetMine(CancellationToken token = default);
        Task<IEnumerable<Subject>> GetAvailable(CancellationToken token = default);
        Task<IEnumerable<Subject>> GetActives(CancellationToken token = default);
    }

    public class SubjectService : DatabaseService<Subject, SubjectRequest>, ISubjectService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IProgressionService _progressionService;
        private readonly IStateService _stateService;

        public SubjectService(LernContext context, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService, IProgressionService progressionService, IStateService stateService) : base(context, httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _progressionService = progressionService;
            _stateService = stateService;
        }

        protected override IQueryable<Subject> WithDefaultIncludes(DbSet<Subject> set)
        {
            return base.WithDefaultIncludes(set)
                .Include(subject => subject.Author)
                .Include(subject => subject.Modules)
                .ThenInclude(module => module.Concepts)
                .ThenInclude(concept => concept.Courses)
                .ThenInclude(course => course.Exercises)
                .ThenInclude(exercise => exercise.Questions)
                .ThenInclude(question => question.Answers)
                .Include(subject => subject.Modules)
                .ThenInclude(module => module.Concepts)
                .ThenInclude(concept => concept.Exercises)
                .ThenInclude(exercise => exercise.Questions)
                .ThenInclude(question => question.Answers);
        }

        public override async Task<Subject> Get(Guid id, CancellationToken token = default)
        {
            var entity = await base.Get(id, token);
            
            if (entity == null)
                return null;

            var canEdit = await _authorizationService.HasWriteAccess(HttpContextAccessor.HttpContext.GetUser(), entity, token);

            if (canEdit)
                return entity;

            return await _stateService.AvailableSubjects.FirstOrDefaultAsync(subject => subject.Id == id, token);
        }

        public override async Task<Subject> Create(SubjectRequest entity, CancellationToken token = default)
        {
            var result = await base.Create(entity, token);

            if (result == null)
                return null;

            return await _stateService.UpdateSubjectState(result.Id, token);
        }

        public override async Task<Subject> Update(Guid id, SubjectRequest entity, CancellationToken token = default)
        {
            var result = await base.Update(id, entity, token);

            if (result == null)
                return null;

            return await _stateService.UpdateSubjectState(result.Id, token);
        }

        public async Task<IEnumerable<Subject>> GetMine(CancellationToken token = default)
        {
            var currentUser = HttpContextAccessor.HttpContext.GetUser();

            return await WithDefaultIncludes(DbSet).Where(x => x.AuthorId == currentUser.Id).ToListAsync(token);
        }

        public async Task<IEnumerable<Subject>> GetAvailable(CancellationToken token = default)
        {
            var currentUser = HttpContextAccessor.HttpContext.GetUser();

            return await _stateService.AvailableSubjects.Where(subject => subject.AuthorId != currentUser.Id).ToListAsync(token);
        }

        public async Task<IEnumerable<Subject>> GetActives(CancellationToken token = default)
        {
            var progressions = await _progressionService.GetAll(HttpContextAccessor.HttpContext.GetUser(), token);

            return _stateService.AvailableSubjects.AsEnumerable().Where(x => progressions.Where(progression => !progression.Suspended).Any(progression => progression.SubjectId == x.Id));
        }
    }
}
