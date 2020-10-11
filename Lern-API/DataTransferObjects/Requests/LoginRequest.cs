using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Lern_API.DataTransferObjects.Requests
{
    public class LoginRequest
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Login).NotNull();
            RuleFor(x => x.Password).NotNull();
        }
    }
}
