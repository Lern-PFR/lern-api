using Lern_API.Controllers;
using Xunit;

namespace Lern_API.Tests.Controllers
{
    public class HelloWorldControllerShould
    {
        [Fact]
        public void Return_Hello_World()
        {
            var controller = new HelloWorldController();

            var result = controller.Index();
            Assert.Equal("Hello, world!", result);
        }
    }
}
