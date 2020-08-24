using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.DataTransferObjects;

namespace Lern_API.Validators
{
    [ExcludeFromCodeCoverage]
    public class UserRequestValidator : AbstractValidator<UserRequest>
    {
        public UserRequestValidator()
        {
            RuleFor(r => r.Id).GreaterThan(0);
        }
    }
}
