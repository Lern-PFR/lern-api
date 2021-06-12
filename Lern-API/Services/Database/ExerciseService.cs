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

        public ExerciseService(LernContext context, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService) : base(context, httpContextAccessor)
        {
            _authorizationService = authorizationService;
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

            var canEdit = await _authorizationService.HasWriteAccess(HttpContextAccessor.HttpContext.GetUser(), entity, token);

            if (canEdit)
                return entity;

            return await DbSet
                .Include(exercise =>
                    exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .ThenInclude(question => question.Answers)
                .FirstOrDefaultAsync(exercise => exercise.Id == id, token);
        }
    }
}
