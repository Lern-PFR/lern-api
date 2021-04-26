using System.Diagnostics.CodeAnalysis;
using FluentValidation;

namespace Lern_API.DataTransferObjects.Requests
{
    public class UserRequest
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
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
                RuleFor(x => x.Firstname).Length(3, 50);
                RuleFor(x => x.Lastname).Length(3, 100);
                RuleFor(x => x.Nickname).Length(3, 50);
                RuleFor(x => x.Email).EmailAddress().MaximumLength(254);
                RuleFor(x => x.Password).Length(8, 100);
            });
        }
    }
}
