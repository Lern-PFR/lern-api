using System;
using FluentValidation;
using Lern_API.Helpers.Database;
using Lern_API.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using PetaPoco;

namespace Lern_API.Models
{
    public enum SubjectState
    {
        Pending = 0,
        Approved = 1
    }

    public class Subject : AbstractModel
    {
        [ReadOnly]
        public Guid? AuthorId { get; set; }
        [ReadOnly]
        [ResultColumn]
        public User Author { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [ReadOnly]
        public SubjectState State { get; set; }
    }

    public class SubjectValidator : AbstractValidator<Subject>
    {
        public SubjectValidator(IService<User> service)
        {
            RuleFor(x => x.AuthorId).NotNull().MustExist(service);
            RuleFor(x => x.Title).NotNull().NotEmpty().Length(3, 50);
            RuleFor(x => x.Description).NotNull().NotEmpty().Length(10, 300);

            RuleSet("Update", () =>
            {
                RuleFor(x => x.Title).Length(3, 50);
                RuleFor(x => x.Description).Length(10, 300);
            });
        }
    }
}
