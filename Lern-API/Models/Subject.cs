using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;

namespace Lern_API.Models
{
    public enum SubjectState
    {
        Pending = 0,
        Approved = 1
    }

    public class Subject : IModelBase
    {
        [ReadOnly(true), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        [ReadOnly(true)]
        public User Author { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [ReadOnly(true)]
        public SubjectState State { get; set; }
    }

    public class SubjectValidator : AbstractValidator<Subject>
    {
        public SubjectValidator()
        {
            RuleFor(x => x.Author).NotNull();
            RuleFor(x => x.Title).NotNull().NotEmpty().Length(3, 50);
            RuleFor(x => x.Description).NotNull().NotEmpty().Length(10, 300);

            RuleSet("Update", () =>
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Title).Length(3, 50);
                RuleFor(x => x.Description).Length(10, 300);
            });
        }
    }
}
