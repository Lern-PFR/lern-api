using System;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.Helpers.Swagger;

namespace Lern_API.Models
{
    public class User : AbstractModel
    {
        [ReadOnly]
        public Guid? Manager { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string Password { internal get; set; }
        [ReadOnly]
        public int? Tokens { get; set; }
        [ReadOnly]
        public int? MaxTopics { get; set; }
        [ReadOnly]
        public bool? Active { get; set; }
        [ReadOnly]
        public bool? Admin { get; set; }
        [ReadOnly]
        public bool? VerifiedCreator { get; set; }
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
