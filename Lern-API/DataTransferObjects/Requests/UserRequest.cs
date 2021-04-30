using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.Models;

namespace Lern_API.DataTransferObjects.Requests
{
    public class UserRequest
    {
        [Required, MinLength(3), MaxLength(50)]
        public string Firstname { get; set; }
        [Required, MinLength(3), MaxLength(100)]
        public string Lastname { get; set; }
        [Required, MinLength(3), MaxLength(50)]
        public string Nickname { get; set; }
        [Required, MaxLength(254)]
        public string Email { get; set; }
        [MinLength(8), MaxLength(100)]
        public string Password { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UserRequestValidator : AbstractValidator<UserRequest>
    {
        public UserRequestValidator()
        {
            RuleFor(x => x.Firstname).NotNull().Length(3, 50);
            RuleFor(x => x.Lastname).NotNull().Length(3, 100);
            RuleFor(x => x.Nickname).NotNull().Length(3, 50);
            RuleFor(x => x.Email).NotNull().EmailAddress().MaximumLength(254);
            RuleFor(x => x.Password).NotNull().Length(8, 100);

            RuleSet("Update", () =>
            {
                RuleFor(x => x.Firstname).NotNull().Length(3, 50);
                RuleFor(x => x.Lastname).NotNull().Length(3, 100);
                RuleFor(x => x.Nickname).NotNull().Length(3, 50);
                RuleFor(x => x.Email).NotNull().EmailAddress().MaximumLength(254);
                RuleFor(x => x.Password).Length(8, 100).Unless(p => p == null);
            });
        }
    }
}
