using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Lern_API.Models
{
    public class User : AbstractModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
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
        }
    }
}
