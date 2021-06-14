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
        IQueryable<Subject> GetAvailable();
        Task<Subject> UpdateState(Guid id, CancellationToken token = default);
    }

    public class SubjectService : DatabaseService<Subject, SubjectRequest>, ISubjectService
    {
        private readonly IAuthorizationService _authorizationService;

        public SubjectService(LernContext context, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService) : base(context, httpContextAccessor)
        {
            _authorizationService = authorizationService;
        }

        protected override IQueryable<Subject> WithDefaultIncludes(DbSet<Subject> set)
        {
            return base.WithDefaultIncludes(set)
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

            return await GetAvailable().FirstOrDefaultAsync(subject => subject.Id == id, token);
        }

        public override async Task<Subject> Create(SubjectRequest entity, CancellationToken token = default)
        {
            var result = await base.Create(entity, token);

            if (result == null)
                return null;

            return await UpdateState(result.Id, token);
        }

        public override async Task<Subject> Update(Guid id, SubjectRequest entity, CancellationToken token = default)
        {
            var result = await base.Update(id, entity, token);

            if (result == null)
                return null;

            return await UpdateState(result.Id, token);
        }

        public override async Task<IEnumerable<Subject>> GetAll(CancellationToken token = default)
        {
            return await GetAvailable().ToListAsync(token);
        }

        public async Task<IEnumerable<Subject>> GetMine(CancellationToken token = default)
        {
            var currentUser = HttpContextAccessor.HttpContext.GetUser();

            return await WithDefaultIncludes(DbSet).Where(x => x.AuthorId == currentUser.Id).ToListAsync(token);
        }

        public IQueryable<Subject> GetAvailable()
        {
            return DbSet
                .Include(subject => subject.Modules.Where(module => module.Concepts.Any()))
                .ThenInclude(module => module.Concepts.Where(concept => concept.Courses.Any() && concept.Exercises.Any(exercise => exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid)))))
                .ThenInclude(concept => concept.Courses)
                .ThenInclude(course => course.Exercises.Where(exercise => exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid))))
                .ThenInclude(exercise => exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .ThenInclude(question => question.Answers)
                .Include(subject => subject.Modules.Where(module => module.Concepts.Any()))
                .ThenInclude(module => module.Concepts.Where(concept => concept.Courses.Any() && concept.Exercises.Any(exercise => exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid)))))
                .ThenInclude(concept => concept.Exercises.Where(exercise => exercise.Questions.Any()))
                .ThenInclude(exercise => exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .ThenInclude(question => question.Answers)
                .Where(subject =>
                    subject.Modules.Any() && subject.Modules.All(module =>
                        module.Concepts.Any() && module.Concepts.All(concept => concept.Courses.Any() && concept.Exercises.Any() && concept.Exercises.All(exercise =>
                            exercise.Questions.Any() && exercise.Questions.All(question => question.Answers.Any(answer => answer.Valid))
                    ))
                ));
        }
        
        public async Task<Subject> UpdateState(Guid id, CancellationToken token = default)
        {
            var subject = await base.Get(id, token);

            if (subject == null)
                return null;

            return await SafeExecute(_ =>
            {
                subject.State = GetAvailable().Any(x => x.Id == id)
                        ? SubjectState.Approved
                        : SubjectState.Invalid;

                return subject;
            }, token);
        }
    }
}
