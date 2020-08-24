using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using FluentValidation;
using Lern_API.DataTransferObjects;

namespace Lern_API.Validators
{
    [ExcludeFromCodeCoverage]
    public class IndexRequestValidator : AbstractValidator<IndexRequest>
    {
        private static readonly Regex PathRegex = new Regex(@"^\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)$", RegexOptions.Compiled);

        public IndexRequestValidator()
        {
            RuleFor(r => r.Path).Matches(PathRegex);
        }
    }
}
