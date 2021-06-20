using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services.Database
{
    public interface IProgressionService : IAbstractDatabaseService<Progression>
    {
        Task<IEnumerable<Progression>> GetAll(User user, CancellationToken token = default);
        Task<Progression> Get(User user, Subject subject, CancellationToken token = default);
        Task<bool> Create(User user, Subject subject, Concept concept, CancellationToken token = default);
        Task<bool> Update(User user, Subject subject, Concept concept, CancellationToken token = default);
        Task<bool> Exists(User user, Subject subject, CancellationToken token = default);
    }

    public class ProgressionService : AbstractDatabaseService<Progression>, IProgressionService
    {
        public ProgressionService(LernContext context) : base(context)
        {
        }

        protected override IQueryable<Progression> WithDefaultIncludes(DbSet<Progression> set)
        {
            return base.WithDefaultIncludes(set)
                .Include(progression => progression.Concept)
                .Include(progression => progression.Subject)
                .ThenInclude(subject => subject.Modules.Where(module => module.Concepts.Any()))
                .ThenInclude(module => module.Concepts.Where(concept =>
                    concept.Lessons.Any() && concept.Exercises.Any(exercise =>
                        exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid)))))
                .Include(progression => progression.User);
        }

        public async Task<bool> Create(User user, Subject subject, Concept concept, CancellationToken token = default)
        {
            var final = new Progression
            {
                UserId = user.Id,
                SubjectId = subject.Id,
                ConceptId = concept.Id
            };

            var entityEntry = await SafeExecute(async set => await set.AddAsync(final, token), token);

            return entityEntry?.Entity != null;
        }

        public async Task<bool> Exists(User user, Subject subject, CancellationToken token = default)
        {
            return await DbSet.AnyAsync(x => x.UserId == user.Id && x.SubjectId == subject.Id, token);
        }

        public async Task<Progression> Get(User user, Subject subject, CancellationToken token = default)
        {
            return await WithDefaultIncludes(DbSet).FirstOrDefaultAsync(x => x.UserId == user.Id && x.SubjectId == subject.Id, token);
        }

        public async Task<IEnumerable<Progression>> GetAll(User user, CancellationToken token = default)
        {
            return await WithDefaultIncludes(DbSet).Where(x => x.UserId == user.Id).ToListAsync(token);
        }

        public async Task<bool> Update(User user, Subject subject, Concept concept, CancellationToken token = default)
        {
            var entry = await Get(user, subject, token);

            bool result;

            if (entry == null)
            {
                result = await Create(user, subject, concept, token);
            }
            else
            {
                result = await SafeExecute(_ =>
                {
                    entry.ConceptId = concept.Id;
                    entry.Concept = concept;
                }, token);
            }

            return result;
        }
    }
}
