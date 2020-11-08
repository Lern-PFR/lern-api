using System;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.Helpers.Database;
using Lern_API.Services;
using PetaPoco;

namespace Lern_API.Models
{
    public class Course : AbstractModel
    {
        [ReadOnly]
        public int Version { get; set; }
        [ReadOnly]
        public Guid? ConceptId { get; set; }
        [ReadOnly]
        [ResultColumn]
        public Concept Concept { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class CourseValidator : AbstractValidator<Course>
    {
        public CourseValidator(IService<Concept> service)
        {
            RuleFor(x => x.ConceptId).NotNull().MustExist(service);
            RuleFor(x => x.Title).NotNull().Length(3, 50);
            RuleFor(x => x.Description).NotNull().Length(10, 300);
            RuleFor(x => x.Content).NotNull();
            RuleFor(x => x.Order).NotNull().GreaterThanOrEqualTo(0);

            RuleSet("Update", () =>
            {
                RuleFor(x => x.Title).Length(3, 50);
                RuleFor(x => x.Description).Length(10, 300);
                RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
            });
        }
    }
}
