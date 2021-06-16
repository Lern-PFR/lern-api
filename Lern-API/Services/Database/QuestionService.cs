using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Helpers.Models;
using Lern_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services.Database
{
    public interface IQuestionService : IDatabaseService<Question, QuestionRequest>
    {

    }

    public class QuestionService : DatabaseService<Question, QuestionRequest>, IQuestionService
    {
        private readonly IStateService _stateService;

        public QuestionService(LernContext context, IHttpContextAccessor httpContextAccessor, IStateService stateService) : base(context, httpContextAccessor)
        {
            _stateService = stateService;
        }

        protected override IQueryable<Question> WithDefaultIncludes(DbSet<Question> set)
        {
            return base.WithDefaultIncludes(set)
                .Include(question => question.Answers);
        }

        public override async Task<Question> Create(QuestionRequest entity, CancellationToken token = default)
        {
            var final = new Question();
            final.CloneFrom(entity);

            final.Answers = entity.Answers.Select(x =>
            {
                var answer = new Answer();
                answer.CloneFrom(x);
                return answer;
            }).ToList();

            var result = await SafeExecute(async set => await set.AddAsync(final, token), token);

            if (result?.Entity == null)
                return null;

            var subject = await Context.Subjects.FirstOrDefaultAsync(x =>
                    x.Modules.Any(module => module.Concepts.Any(concept => concept.Exercises.Any(exercise => exercise.Id == result.Entity.ExerciseId)) ||
                                            module.Concepts.Any(concept => concept.Courses.Any(course => course.Exercises.Any(exercise => exercise.Id == result.Entity.ExerciseId)))),
                token
            );

            await _stateService.UpdateSubjectState(subject?.Id ?? default, token);

            return result.Entity;
        }

        public override async Task<Question> Update(Guid id, QuestionRequest entity, CancellationToken token = default)
        {
            var storedQuestion = await Get(id, token);
            storedQuestion.CloneFrom(entity);

            if (entity.Answers != null)
            {
                var newAnswers = new List<Answer>();

                foreach (var answer in entity.Answers)
                {
                    var newAnswer = new Answer();
                    newAnswer.CloneFrom(answer);
                    newAnswer.Id = Guid.NewGuid();
                    newAnswers.Add(newAnswer);
                }

                storedQuestion.Answers.Clear();
                storedQuestion.Answers.AddRange(newAnswers);
            }

            var result = await SafeExecute(set => set.Update(storedQuestion), token);

            if (result?.Entity == null)
                return null;

            var subject = await Context.Subjects.FirstOrDefaultAsync(x =>
                    x.Modules.Any(module => module.Concepts.Any(concept => concept.Exercises.Any(exercise => exercise.Id == result.Entity.ExerciseId)) ||
                                            module.Concepts.Any(concept => concept.Courses.Any(course => course.Exercises.Any(exercise => exercise.Id == result.Entity.ExerciseId)))),
                token
            );

            await _stateService.UpdateSubjectState(subject?.Id ?? default, token);

            return result.Entity;
        }

        public override async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            var entity = await base.Get(id, token);
            var result = await base.Delete(id, token);

            if (!result)
                return false;

            var subject = await Context.Subjects.FirstOrDefaultAsync(x =>
                x.Modules.Any(module => module.Concepts.Any(concept => concept.Exercises.Any(exercise => exercise.Id == entity.ExerciseId)) ||
                                        module.Concepts.Any(concept => concept.Courses.Any(course => course.Exercises.Any(exercise => exercise.Id == entity.ExerciseId)))),
                token
            );

            await _stateService.UpdateSubjectState(subject?.Id ?? default, token);

            return true;
        }
    }
}
