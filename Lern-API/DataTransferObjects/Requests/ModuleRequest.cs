using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.Helpers.Validation;
using Lern_API.Models;
using Lern_API.Services;

namespace Lern_API.DataTransferObjects.Requests
{
    public class ModuleRequest
    {
        [Required]
        public Guid SubjectId { get; set; }
        [Required, MinLength(3), MaxLength(50)]
        public string Title { get; set; }
        [Required, MinLength(10), MaxLength(300)]
        public string Description { get; set; }
        [Required]
        public int Order { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ModuleRequestValidator : AbstractValidator<ModuleRequest>
    {
        public ModuleRequestValidator(IDatabaseService<Subject, SubjectRequest> subjectService)
        {
            RuleFor(x => x.SubjectId).NotEmpty().MustExistInDatabase(subjectService);
            RuleFor(x => x.Title).NotNull().Length(3, 50);
            RuleFor(x => x.Description).NotNull().Length(10, 300);
            RuleFor(x => x.Order).NotNull().GreaterThanOrEqualTo(0);

            RuleSet("Update", () =>
            {
                RuleFor(x => x.SubjectId).NotEmpty().MustExistInDatabase(subjectService);
                RuleFor(x => x.Title).NotNull().Length(3, 50);
                RuleFor(x => x.Description).NotNull().Length(10, 300);
                RuleFor(x => x.Order).NotNull().GreaterThanOrEqualTo(0);
            });
        }
    }
}
