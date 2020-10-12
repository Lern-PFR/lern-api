using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Models;
using Lern_API.Repositories;
using Lern_API.Services;
using Lern_API.Tests.Attributes;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class UserServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_User_By_Login(ILogger<UserService> logger, Mock<IUserRepository> repository, LoginRequest request)
        {
            User user = null;
            repository.Setup(x => x.GetByLogin(request.Login)).ReturnsAsync(user);

            var service = new UserService(logger, repository.Object);
            var result = await service.Login(request);

            Assert.Null(result);
            repository.VerifyAll();
        }
    }
}
