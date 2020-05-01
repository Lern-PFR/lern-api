using System.Threading.Tasks;
using Lern_API.Tests.Attributes;
using Lern_API.Utilities;
using Nancy;
using Nancy.Testing;
using Xunit;

namespace Lern_API.Tests.Modules
{
    public class IndexModuleShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Return_Status_OK(ILogger logger)
        {
            var browser = new Browser(new LernBootstrapper(logger));

            var result = await browser.Get("/", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Html_View(ILogger logger)
        {
            var browser = new Browser(new LernBootstrapper(logger));

            var result = await browser.Get("/", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal("text/html", result.ContentType);
        }
    }
}
