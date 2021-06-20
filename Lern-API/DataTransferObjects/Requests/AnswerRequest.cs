using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.Models;
using Lern_API.Services.Database;

namespace Lern_API.DataTransferObjects.Requests
{
    public class AnswerRequest
    {
        [Required, MinLength(1), MaxLength(300)]
        public string Text { get; set; }
        [Required]
        public bool Valid { get; set; }
    }
    
    [ExcludeFromCodeCoverage]
    public class AnswerRequestValidator : AbstractValidator<AnswerRequest>
    {
        public AnswerRequestValidator(IDatabaseService<Question, QuestionRequest> questionService)
        {
            RuleFor(x => x.Text).NotEmpty().Length(1, 300);
            
            RuleSet("Update", () =>
            {
                RuleFor(x => x.Text).NotEmpty().Length(1, 300);
            });
        }
    }
}
