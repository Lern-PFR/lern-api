using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;

namespace Lern_API.Models
{
    [ExcludeFromCodeCoverage]
    public class Identity : IIdentity
    {
        public string AuthenticationType { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
    }
}
