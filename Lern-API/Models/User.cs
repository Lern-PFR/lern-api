using System.Security.Principal;

namespace Lern_API.Models
{
    public class User : IIdentity
    {
        public string AuthenticationType { get; }
        public bool IsAuthenticated { get; }
        public string Name { get; }
    }
}
