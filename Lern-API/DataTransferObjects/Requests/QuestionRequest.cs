using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.Helpers.Validation;
using Lern_API.Models;
using Lern_API.Services.Database;

namespace Lern_API.DataTransferObjects.Requests
{
    public class QuestionRequest
    {
        [Required]
        public Guid ExerciseId { get; set; }
        [Required]
        public QuestionType Type { get; set; }
        [Required, MinLength(3), MaxLength(300)]
        public string Statement { get; set; }
        [MaxLength(3000)]
        public string Explanation { get; set; }
        [Required]
        public List<AnswerRequest> Answers { get; set; }
    }
    
    [ExcludeFromCodeCoverage]
    public class QuestionRequestValidator : AbstractValidator<QuestionRequest>
    {
        public QuestionRequestValidator(IDatabaseService<Exercise, ExerciseRequest> exerciseService)
        {
            RuleFor(x => x.ExerciseId).NotNull().MustExistInDatabase(exerciseService);
            RuleFor(x => x.Statement).NotEmpty().Length(3, 300);
            RuleFor(x => x.Explanation).MaximumLength(3000).Unless(x => string.IsNullOrWhiteSpace(x.Explanation));
            RuleFor(x => x.Answers).NotEmpty();
            
            RuleSet("Update", () =>
            {
                RuleFor(x => x.ExerciseId).NotNull().MustExistInDatabase(exerciseService);
                RuleFor(x => x.Statement).NotEmpty().Length(3, 300);
                RuleFor(x => x.Explanation).MaximumLength(3000).Unless(x => string.IsNullOrWhiteSpace(x.Explanation));
                RuleFor(x => x.Answers).NotEmpty();
            });
        }
    }
}
