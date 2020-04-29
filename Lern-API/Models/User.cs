using System.Security.Principal;

namespace Lern_API.Models
{
    public class User : IIdentity
    {
        public string AuthenticationType { get; set;  }
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
    }
}
