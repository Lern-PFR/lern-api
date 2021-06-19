using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Helpers.JWT;
using Lern_API.Helpers.Models;
using Lern_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services.Database
{
    public interface ILessonService : IDatabaseService<Lesson, LessonRequest>
    {
        Task<Lesson> GetExact(Guid id, int version, CancellationToken token = default);
    }

    public class LessonService : DatabaseService<Lesson, LessonRequest>, ILessonService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IStateService _stateService;

        public LessonService(LernContext context, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService, IStateService stateService) : base(context, httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _stateService = stateService;
        }

        protected override IQueryable<Lesson> WithDefaultIncludes(DbSet<Lesson> set)
        {
            return base.WithDefaultIncludes(set)
                .Include(lesson => lesson.Exercises)
                .ThenInclude(exercise => exercise.Questions)
                .ThenInclude(question => question.Answers);
        }

        public override async Task<Lesson> Get(Guid id, CancellationToken token = default)
        {
            var set = WithDefaultIncludes(DbSet);

            var entity = await set.FirstOrDefaultAsync(x => x.Id == id && x.Version == set.Where(lesson => lesson.Id == id).Max(lesson => lesson.Version), token);

            if (entity == null)
                return null;

            var canEdit = await _authorizationService.HasWriteAccess(HttpContextAccessor.HttpContext.GetUser(), entity, token);

            if (canEdit)
                return entity;

            return await DbSet
                .Include(lesson =>
                    lesson.Exercises.Where(exercise => exercise.Questions.Any()))
                .ThenInclude(exercise =>
                    exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .ThenInclude(question => question.Answers)
                .Where(lesson => lesson.Exercises.All(exercise => exercise.Questions.Any() && exercise.Questions.All(question => question.Answers.Any(answer => answer.Valid))))
                .FirstOrDefaultAsync(lesson => lesson.Id == id && lesson.Version == entity.Version, token);
        }

        public override async Task<Lesson> Create(LessonRequest entity, CancellationToken token = default)
        {
            var result = await base.Create(entity, token);

            if (result == null)
                return null;

            var subject = await Context.Subjects.FirstOrDefaultAsync(x => x.Modules.Any(module => module.Concepts.Any(concept => concept.Id == result.ConceptId)), token);
            await _stateService.UpdateSubjectState(subject?.Id ?? default, token);

            return result;
        }

        public override async Task<Lesson> Update(Guid id, LessonRequest entity, CancellationToken token = default)
        {
            var entry = await Get(id, token);
            
            var newVersion = new Lesson();
            newVersion.CloneFrom(entry);
            newVersion.CloneFrom(entity);
            newVersion.Version += 1;

            var result = await SafeExecute(async set =>
            {
                // Retrieve all exercises associated to the lesson to clone them and their children
                var exercises = Context.Exercises.Where(exercise => exercise.LessonId == entry.Id && exercise.LessonVersion == entry.Version)
                    .Include(exercise => exercise.Questions)
                    .ThenInclude(question => question.Answers)
                    .ToList();

                newVersion.Exercises = new List<Exercise>(exercises.Count);

                foreach (var exercise in exercises)
                {
                    var newExercise = new Exercise();
                    newExercise.CloneFrom(exercise);

                    newExercise.Id = default;
                    newExercise.LessonId = default;
                    newExercise.LessonVersion = default;
                    newExercise.Questions = new List<Question>(exercise.Questions.Count);

                    foreach (var question in exercise.Questions)
                    {
                        var newQuestion = new Question();
                        newQuestion.CloneFrom(question);

                        newQuestion.Id = default;
                        newQuestion.ExerciseId = default;
                        newQuestion.Answers = new List<Answer>(question.Answers.Count);

                        foreach (var answer in question.Answers)
                        {
                            var newAnswer = new Answer();
                            newAnswer.CloneFrom(answer);

                            newAnswer.Id = Guid.NewGuid();
                            newAnswer.QuestionId = default;

                            newQuestion.Answers.Add(newAnswer);
                        }

                        newExercise.Questions.Add(newQuestion);
                    }

                    newVersion.Exercises.Add(newExercise);
                }

                return await set.AddAsync(newVersion, token);
            }, token);
            
            if (result?.Entity == null)
                return null;

            var subject = await Context.Subjects.FirstOrDefaultAsync(x => x.Modules.Any(module => module.Concepts.Any(concept => concept.Id == result.Entity.ConceptId)), token);
            await _stateService.UpdateSubjectState(subject?.Id ?? default, token);

            return result.Entity;
        }

        public override async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            var entity = await Get(id, token);
            var result = await SafeExecute(set => set.RemoveRange(set.Where(x => x.Id == id)), token);

            if (!result)
                return false;

            var subject = await Context.Subjects.FirstOrDefaultAsync(x => x.Modules.Any(module => module.Concepts.Any(concept => concept.Id == entity.ConceptId)), token);
            await _stateService.UpdateSubjectState(subject?.Id ?? default, token);

            return true;
        }

        public async Task<Lesson> GetExact(Guid id, int version, CancellationToken token = default)
        {
            return await WithDefaultIncludes(DbSet).FirstOrDefaultAsync(x => x.Id == id && x.Version == version, token);
        }
    }
}
