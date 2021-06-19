using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.Helpers.Validation;
using Lern_API.Models;
using Lern_API.Services.Database;

namespace Lern_API.DataTransferObjects.Requests
{
    public class ExerciseRequest
    {
        public Guid? ConceptId { get; set; }
        public Guid? LessonId { get; set; }
        [Required, MinLength(3), MaxLength(50)]
        public string Title { get; set; }
        [Required, MinLength(10), MaxLength(300)]
        public string Description { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public int Order { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ExerciseRequestValidator : AbstractValidator<ExerciseRequest>
    {
        public ExerciseRequestValidator(IDatabaseService<Concept, ConceptRequest> conceptService, IDatabaseService<Lesson, LessonRequest> lessonService)
        {
            RuleFor(x => x.ConceptId).MustExistInDatabaseIfNotNull(conceptService).NotNull().When(e => e.LessonId == null).WithMessage("ConceptId and LessonId cannot be both null");
            RuleFor(x => x.LessonId).MustExistInDatabaseIfNotNull(lessonService).NotNull().When(e => e.ConceptId == null).WithMessage("ConceptId and LessonId cannot be both null");
            RuleFor(x => x.Title).NotNull().Length(3, 50);
            RuleFor(x => x.Description).NotNull().Length(10, 300);
            RuleFor(x => x.Content).NotNull();
            RuleFor(x => x.Order).NotNull().GreaterThanOrEqualTo(0);

            RuleSet("Update", () =>
            {
                RuleFor(x => x.ConceptId).MustExistInDatabaseIfNotNull(conceptService).NotNull().When(e => e.LessonId == null).WithMessage("ConceptId and LessonId cannot be both null");
                RuleFor(x => x.LessonId).MustExistInDatabaseIfNotNull(lessonService).NotNull().When(e => e.ConceptId == null).WithMessage("ConceptId and LessonId cannot be both null");
                RuleFor(x => x.Title).NotNull().Length(3, 50);
                RuleFor(x => x.Description).NotNull().Length(10, 300);
                RuleFor(x => x.Order).NotNull().GreaterThanOrEqualTo(0);
            });
        }
    }
}
