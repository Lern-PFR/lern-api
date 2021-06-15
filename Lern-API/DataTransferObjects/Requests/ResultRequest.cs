using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.Helpers.Validation;
using Lern_API.Models;
using Lern_API.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.DataTransferObjects.Requests
{
    public class ResultRequest
    {
        [Required]
        public Guid QuestionId { get; set; }
        [Required]
        public Guid AnswerId { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ResultRequestValidator : AbstractValidator<ResultRequest>
    {
        public ResultRequestValidator(IQuestionService questionService, IDatabaseService<Answer, AnswerRequest> answerService)
        {
            RuleFor(x => x.QuestionId).NotNull().MustExistInDatabase(questionService);
            RuleFor(x => x.AnswerId).NotNull().MustExistInDatabase(answerService)
                .MustAsync(async (request, answerId, token) => await answerService.ExecuteQuery(async set => await set.AnyAsync(answer => answer.Id == answerId && answer.QuestionId == request.QuestionId, token), token))
                .WithMessage("Provided Answer does not belong to provided Question");
        }
    }
}
