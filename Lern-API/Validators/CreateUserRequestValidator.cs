using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.DataTransferObjects;

namespace Lern_API.Validators
{
    [ExcludeFromCodeCoverage]
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(r => r.Name).MinimumLength(3).MaximumLength(50);
        }
    }
}
