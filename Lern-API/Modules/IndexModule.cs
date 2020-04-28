using Nancy;

namespace Lern_API.Modules
{
    public sealed class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get("/", _ => Response.AsFile("Content/index.html", "text/html"));
        }
    }
}
