using System.Threading.Tasks;
using Nancy;
using Nancy.Testing;
using Xunit;

namespace Lern_API.Tests.Modules
{
    public class IndexModuleShould
    {
        [Fact]
        public async Task Return_Status_OK()
        {
            var browser = new Browser(new LernBootstrapper());

            var result = await browser.Get("/", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task Return_Html_View()
        {
            var browser = new Browser(new LernBootstrapper());

            var result = await browser.Get("/", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal("text/html", result.ContentType);
        }
    }
}
