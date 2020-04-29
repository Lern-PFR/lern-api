using Nancy;
using Nancy.Security;

namespace Lern_API.Modules
{
    public sealed class IdentityModule : NancyModule
    {
        public IdentityModule() : base("/me")
        {
            this.RequiresAuthentication();

            Get("", _ => Context.CurrentUser.Identity.Name);
        }
    }
}
