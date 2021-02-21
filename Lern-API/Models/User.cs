using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;

namespace Lern_API.Models
{
    public class User
    {
        [ReadOnly(true), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        [ReadOnly(true)]
        public User Manager { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string Password { internal get; set; }
        [ReadOnly(true), DefaultValue(0)]
        public int Tokens { get; set; }
        [ReadOnly(true), DefaultValue(5)]
        public int MaxSubjects { get; set; }
        [ReadOnly(true), DefaultValue(true)]
        public bool Active { get; set; }
        [ReadOnly(true), DefaultValue(false)]
        public bool Admin { get; set; }
        [ReadOnly(true), DefaultValue(false)]
        public bool VerifiedCreator { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(x => x.Firstname).NotNull().Length(3, 50);
            RuleFor(x => x.Lastname).NotNull().Length(3, 100);
            RuleFor(x => x.Nickname).NotNull().Length(3, 50);
            RuleFor(x => x.Email).NotNull().EmailAddress().MaximumLength(254);
            RuleFor(x => x.Password).NotNull().Length(8, 100);

            RuleSet("Update", () =>
            {
                RuleFor(x => x.Firstname).Length(3, 50);
                RuleFor(x => x.Lastname).Length(3, 100);
                RuleFor(x => x.Nickname).Length(3, 50);
                RuleFor(x => x.Email).EmailAddress().MaximumLength(254);
                RuleFor(x => x.Password).Length(8, 100);
            });
        }
    }
}
