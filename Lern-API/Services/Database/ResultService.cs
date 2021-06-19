using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services.Database
{
    public interface IResultService : IAbstractDatabaseService<Result>
    {
        Task<IEnumerable<Result>> GetAll(User user, Subject subject, CancellationToken token = default);
        Task<IEnumerable<Result>> GetAll(User user, Exercise exercise, CancellationToken token = default);
        Task<Result> Get(User user, Question question, CancellationToken token = default);
    }

    public class ResultService : AbstractDatabaseService<Result>, IResultService
    {
        public ResultService(LernContext context) : base(context)
        {
        }

        protected override IQueryable<Result> WithDefaultIncludes(DbSet<Result> set)
        {
            return base.WithDefaultIncludes(set)
                .Include(result => result.Answer)   
                .Include(result => result.User);
        }
        
        public async Task<IEnumerable<Result>> GetAll(User user, Subject subject, CancellationToken token = default)
        {
            var results = await WithDefaultIncludes(DbSet).Where(x => x.UserId == user.Id).ToListAsync(token);

            return results.Where(x => subject.Modules.Any(module =>
                module.Concepts.Any(concept =>
                   concept.Exercises.Any(exercise =>
                       exercise.Questions.Any(question => question.Id == x.QuestionId)) ||
                   concept.Lessons.Any(lesson =>
                       lesson.Exercises.Any(exercise =>
                           exercise.Questions.Any(question => question.Id == x.QuestionId)
                )))));
        }

        public async Task<IEnumerable<Result>> GetAll(User user, Exercise exercise, CancellationToken token = default)
        {
            var results = await WithDefaultIncludes(DbSet).Where(x => x.UserId == user.Id).ToListAsync(token);

            return results.Where(x => exercise.Questions.Any(question => question.Id == x.QuestionId));
        }

        public async Task<Result> Get(User user, Question question, CancellationToken token = default)
        {
            return await WithDefaultIncludes(DbSet).FirstOrDefaultAsync(x => x.UserId == user.Id && x.QuestionId == question.Id, token);
        }
    }
}
