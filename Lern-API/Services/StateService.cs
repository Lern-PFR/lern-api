using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Filters;
using Lern_API.Models;
using Lern_API.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services
{
    public interface IStateService
    {
        IQueryable<Subject> AvailableSubjects { get; }
        Task<Subject> UpdateSubjectState(Guid id, CancellationToken token = default);
        Task<bool> UpdateCompletion(User user, Guid subjectId, CancellationToken token = default);
        Task UpdateCompletions(Guid subjectId, CancellationToken token = default);
        Task<bool> RegisterAnswers(User user, IEnumerable<ResultRequest> answers, CancellationToken token = default);
    }

    public class StateService : AbstractDatabaseService<Subject>, IStateService
    {
        private readonly IProgressionService _progressionService;
        private readonly IResultService _resultService;

        public IQueryable<Subject> AvailableSubjects => SubjectsFilters.ValidSubjects(DbSet);

        public StateService(LernContext context, IProgressionService progressionService, IResultService resultService) : base(context)
        {
            _progressionService = progressionService;
            _resultService = resultService;
        }

        public async Task<bool> UpdateCompletion(User user, Guid subjectId, CancellationToken token = default)
        {
            var subject = await AvailableSubjects.FirstOrDefaultAsync(x => x.Id == subjectId, token);

            if (subject == null)
                return false;

            if (!await _progressionService.Exists(user, subject, token))
            {
                var firstModule = subject.Modules.First(x => x.Order == subject.Modules.Min(module => module.Order));
                var firstConcept = firstModule.Concepts.First(x => x.Order == firstModule.Concepts.Min(concept => concept.Order));

                await _progressionService.Create(user, subject, firstConcept, token);
            }

            var entry = await _progressionService.Get(user, subject, token);
            var results = (await _resultService.GetAll(user, subject, token)).ToList();

            var validResultsCount = results.Count(result => result.Answer.Valid);
            var resultsCount = results.Count;
            
            var questionsCount =
                subject.Modules.Sum(module =>
                    module.Concepts.Sum(concept =>
                        concept.Exercises.Sum(exercise => exercise.Questions.Count) + concept.Lessons.Sum(lesson => lesson.Exercises.Sum(exercise => exercise.Questions.Count))));

            var completion = (resultsCount * 100d) / questionsCount;
            var score = (validResultsCount * 100d) / questionsCount;

            return await _progressionService.ExecuteTransaction(set =>
            {
                entry.Completion = completion;
                entry.Completed = resultsCount == questionsCount;

                if (entry.Completed)
                    entry.Score = score;
                else
                    entry.Score = 0;
            }, token);
        }

        public async Task UpdateCompletions(Guid subjectId, CancellationToken token = default)
        {
            var progressions = _progressionService.ExecuteQuery(set => set.Where(x => x.SubjectId == subjectId));

            foreach (var progression in progressions)
            {
                await UpdateCompletion(progression.User, subjectId, token);
            }
        }

        public async Task<bool> RegisterAnswers(User user, IEnumerable<ResultRequest> answers, CancellationToken token = default)
        {
            var results = answers.Select(answer => new Result
            {
                AnswerId = answer.AnswerId,
                QuestionId = answer.QuestionId,
                UserId = user.Id
            }).ToList();

            var transaction = await _resultService.ExecuteTransaction(async set => await set.AddRangeAsync(results, token), token);

            if (!transaction)
                return false;

            var subjects = AvailableSubjects.AsEnumerable().Where(subject => subject.Modules.Any(module =>
                module.Concepts.Any(concept =>
                    concept.Exercises.Any(exercise =>
                        exercise.Questions.Any(question => results.Any(result => result.QuestionId == question.Id))) ||
                    concept.Lessons.Any(lesson =>
                        lesson.Exercises.Any(exercise =>
                            exercise.Questions.Any(question => results.Any(result => result.QuestionId == question.Id))))
                    )));

            foreach (var subject in subjects)
            {
                await UpdateCompletion(user, subject.Id, token);
            }

            return true;
        }

        public async Task<Subject> UpdateSubjectState(Guid id, CancellationToken token = default)
        {
            var subject = await DbSet.FindAsync(new object[] { id }, token);

            if (subject == null)
                return null;

            var stateUpdate = await ExecuteTransaction(_ =>
            {
                subject.State = AvailableSubjects.Any(x => x.Id == id)
                    ? SubjectState.Approved
                    : SubjectState.Invalid;

                return subject;
            }, token);
            
            if (subject.State != SubjectState.Approved)
                return subject;

            await UpdateCompletions(subject.Id, token);

            return stateUpdate;
        }
    }
}
