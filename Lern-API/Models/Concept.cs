using System;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.Helpers.Database;
using Lern_API.Services;
using PetaPoco;

namespace Lern_API.Models
{
    public class Concept : AbstractModel
    {
        [ReadOnly]
        public Guid? ModuleId { get; set; }
        [ReadOnly]
        [ResultColumn]
        public Module Module { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ConceptValidator : AbstractValidator<Concept>
    {
        public ConceptValidator(IService<Module> service)
        {
            RuleFor(x => x.ModuleId).NotNull().MustExist(service);
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
