using System.Threading.Tasks;
using Lern_API.Tests.Attributes;
using Lern_API.Utilities;
using Nancy;
using Nancy.Testing;
using PetaPoco;
using Xunit;

namespace Lern_API.Tests.Modules
{
    public class IndexModuleShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Return_Status_OK(ILogger logger, IDatabase database)
        {
            var browser = new Browser(new LernBootstrapper(logger, database));

            var result = await browser.Get("/", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Html_View(ILogger logger, IDatabase database)
        {
            var browser = new Browser(new LernBootstrapper(logger, database));

            var result = await browser.Get("/", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal("text/html", result.ContentType);
        }
    }
}
