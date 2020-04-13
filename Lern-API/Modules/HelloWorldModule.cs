using Nancy;

namespace Lern_API.Modules
{
    public sealed class HelloWorldModule : NancyModule
    {
        public HelloWorldModule() : base("/hello")
        {
            Get("", _ => "Hello, world!");
        }
    }
}
