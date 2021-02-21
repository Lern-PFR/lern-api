using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;

namespace Lern_API.Models
{
    public class Concept
    {
        [ReadOnly(true), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        [ReadOnly(true)]
        public Module Module { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ConceptValidator : AbstractValidator<Concept>
    {
        public ConceptValidator()
        {
            RuleFor(x => x.Module).NotNull();
            RuleFor(x => x.Title).NotNull().Length(3, 50);
            RuleFor(x => x.Description).NotNull().Length(10, 300);
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
