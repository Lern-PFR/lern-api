using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Lern_API.Helpers.Validation;
using Lern_API.Models;
using Lern_API.Services;

namespace Lern_API.DataTransferObjects.Requests
{
    public class QuestionRequest
    {
        [Required]
        public Guid ExerciseId { get; set; }
        [Required, EnumDataType(typeof(QuestionType))]
        public QuestionType Type { get; set; }
        [Required, MinLength(3), MaxLength(300)]
        public string Statement { get; set; }
        [Required, MinLength(10), MaxLength(3000)]
        public string Explanation { get; set; }
        [Required]
        public List<Answer> Answers { get; set; }
    }
    
    public class QuestionRequestValidator : AbstractValidator<QuestionRequest>
    {
        public QuestionRequestValidator(IService<Exercise, ExerciseRequest> exerciseService)
        {
            RuleFor(x => x.ExerciseId).NotNull().MustExistInDatabase(exerciseService);
            RuleFor(x => x.Statement).NotEmpty().Length(3, 300);
            RuleFor(x => x.Explanation).NotEmpty().Length(10, 3000);
            RuleFor(x => x.Answers).NotEmpty();
            
            RuleSet("Update", () =>
            {
                RuleFor(x => x.ExerciseId).NotNull().MustExistInDatabase(exerciseService);
                RuleFor(x => x.Statement).NotEmpty().Length(3, 300);
                RuleFor(x => x.Explanation).NotEmpty().Length(10, 3000);
                RuleFor(x => x.Answers).NotEmpty();
            });
        }
    }
}
