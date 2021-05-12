using System.Diagnostics.CodeAnalysis;
using FluentValidation;

namespace Lern_API.DataTransferObjects.Requests
{
    public class ForgottenPasswordDefinitionRequest
    {
        public string Token { get; set; }
        public string Password { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ForgottenPasswordDefinitionRequestValidator : AbstractValidator<ForgottenPasswordDefinitionRequest>
    {
        public ForgottenPasswordDefinitionRequestValidator()
        {
            RuleFor(x => x.Token).NotNull().NotEmpty();
            RuleFor(x => x.Password).NotNull().Length(8, 100);
        }
    }
}
