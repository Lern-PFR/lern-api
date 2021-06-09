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

namespace Lern_API.Services
{
    public interface IQuestionService : IDatabaseService<Question, QuestionRequest>
    {

    }

    public class QuestionService : DatabaseService<Question, QuestionRequest>, IQuestionService
    {
        public QuestionService(LernContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        public new async Task<Question> Create(QuestionRequest entity, CancellationToken token = default)
        {
            var final = new Question();
            final.CloneFrom(entity);

            final.Answers = entity.Answers.Select(x =>
            {
                var answer = new Answer();
                answer.CloneFrom(x);
                return answer;
            }).ToList();

            var entityEntry = await SafeExecute(async set => await set.AddAsync(final, token), token);

            return entityEntry?.Entity;
        }

        public new async Task<Question> Update(Guid id, QuestionRequest entity, CancellationToken token = default)
        {
            var storedQuestion = await Get(id, token);
            storedQuestion.CloneFrom(entity);

            if (entity.Answers != null)
            {
                var newAnswers = new List<Answer>();

                foreach (var answer in entity.Answers)
                {
                    var existingAnswer = storedQuestion.Answers.FirstOrDefault(x => x.Id == answer.Id);

                    if (existingAnswer != null)
                    {
                        existingAnswer.CloneFrom(answer);
                        newAnswers.Add(existingAnswer);
                    }
                    else
                    {
                        var newAnswer = new Answer();
                        newAnswer.CloneFrom(answer);
                        newAnswers.Add(newAnswer);
                    }
                }

                storedQuestion.Answers.Clear();
                storedQuestion.Answers.AddRange(newAnswers);
            }

            var result = await SafeExecute(set => set.Update(storedQuestion), token);

            return result?.Entity;
        }
    }
}
