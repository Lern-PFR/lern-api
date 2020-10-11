using Lern_API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Lern_API.Tests.Controllers
{
    public class DefaultControllerShould
    {
        [Fact]
        public void Return_File_Stream()
        {
            var controller = new DefaultController();

            var result = controller.CatchAll();
            Assert.IsType<FileStreamResult>(result);
        }
    }
}
