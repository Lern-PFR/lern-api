using FluentValidation;

namespace Lern_API.Models
{
    public class User : AbstractModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { internal get; set; }
    }

    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(x => x.Id).Empty();
            RuleFor(x => x.Name).NotNull().Length(3, 50);
            RuleFor(x => x.Email).NotNull().EmailAddress();
            RuleFor(x => x.Password).NotNull().MinimumLength(8);

            RuleSet("Update", () =>
            {
                RuleFor(x => x.Id).Empty();
                RuleFor(x => x.Name).Length(3, 50);
                RuleFor(x => x.Email).EmailAddress();
                RuleFor(x => x.Password).MinimumLength(8);
            });
        }
    }
}
