using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;

namespace Lern_API.DataTransferObjects.Requests
{
    public class SubjectRequest
    {
        [Required, MinLength(3), MaxLength(50)]
        public string Title { get; set; }
        [Required, MinLength(10), MaxLength(300)]
        public string Description { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SubjectRequestValidator : AbstractValidator<SubjectRequest>
    {
        public SubjectRequestValidator()
        {
            RuleFor(x => x.Title).NotNull().NotEmpty().Length(3, 50);
            RuleFor(x => x.Description).NotNull().NotEmpty().Length(10, 300);

            RuleSet("Update", () =>
            {
                RuleFor(x => x.Title).NotNull().Length(3, 50);
                RuleFor(x => x.Description).NotNull().Length(10, 300);
            });
        }
    }
}
