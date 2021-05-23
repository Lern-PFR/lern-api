using System.Diagnostics.CodeAnalysis;
using FluentValidation;

namespace Lern_API.DataTransferObjects.Requests
{
    public class ForgottenPasswordRequest
    {
        public string Token { get; set; }
        public string Password { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ForgottenPasswordRequestValidator : AbstractValidator<ForgottenPasswordRequest>
    {
        public ForgottenPasswordRequestValidator()
        {
            RuleFor(x => x.Token).NotNull().NotEmpty();
            RuleFor(x => x.Password).NotNull().Length(8, 100);
        }
    }
}
