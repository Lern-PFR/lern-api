using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.Helpers.Validation;
using Lern_API.Services.Database;

namespace Lern_API.DataTransferObjects.Requests
{
    public class ProgressionRequest
    {
        [Required]
        public Guid SubjectId { get; set; }
        [Required]
        public Guid ConceptId { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ProgressionRequestValidator : AbstractValidator<ProgressionRequest>
    {
        public ProgressionRequestValidator(ISubjectService subjectService, IConceptService conceptService)
        {
            RuleFor(x => x.SubjectId).NotEmpty().MustExistInDatabase(subjectService);
            RuleFor(x => x.ConceptId).NotEmpty().MustExistInDatabase(conceptService);
        }
    }
}
