using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services
{
    public interface IProgressionService : IAbstractDatabaseService<Progression>
    {
        Task<IEnumerable<Progression>> GetAll(User user, CancellationToken token = default);
        Task<Progression> Get(User user, Subject subject, CancellationToken token = default);
        Task<Progression> Create(User user, Subject subject, Concept concept, CancellationToken token = default);
        Task<Progression> Update(User user, Subject subject, Concept concept, CancellationToken token = default);
        Task<bool> Exists(User user, Subject subject, CancellationToken token = default);
    }

    public class ProgressionService : AbstractDatabaseService<Progression>, IProgressionService
    {
        public ProgressionService(LernContext context) : base(context)
        {
        }

        public virtual async Task<Progression> Create(User user, Subject subject, Concept concept, CancellationToken token = default)
        {
            var final = new Progression
            {
                UserId = user.Id,
                SubjectId = subject.Id,
                ConceptId = concept.Id
            };

            var entityEntry = await SafeExecute(async set => await set.AddAsync(final, token), token);

            return entityEntry?.Entity;
        }

        public virtual async Task<bool> Exists(User user, Subject subject, CancellationToken token = default)
        {
            return await DbSet.AnyAsync(x => x.UserId == user.Id && x.SubjectId == subject.Id, token);
        }

        public virtual async Task<Progression> Get(User user, Subject subject, CancellationToken token = default)
        {
            return await DbSet.FirstOrDefaultAsync(x => x.UserId == user.Id && x.SubjectId == subject.Id, token);
        }

        public virtual async Task<IEnumerable<Progression>> GetAll(User user, CancellationToken token = default)
        {
            return await DbSet.Where(x => x.UserId == user.Id).ToListAsync(token);
        }

        public virtual async Task<Progression> Update(User user, Subject subject, Concept concept, CancellationToken token = default)
        {
            var entry = await Get(user, subject, token);

            entry.ConceptId = concept.Id;
            entry.Concept = concept;

            var result = await SafeExecute(set => set.Update(entry), token);

            return result?.Entity;
        }
    }
}
