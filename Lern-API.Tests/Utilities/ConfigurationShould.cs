using Lern_API.Tests.Attributes;
using Lern_API.Utilities;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Lern_API.Tests.Utilities
{
    public class ConfigurationShould
    {
        [Theory]
        [AutoMoqData]
        public void Return_Empty_Value_If_Key_Not_Found(IConfiguration configuration, string key)
        {
            Configuration.Config = configuration;

            Assert.Null(Configuration.Get<string>(key));
            Assert.Equal(0, Configuration.Get<int>(key));
            Assert.Empty(Configuration.GetList(key));
        }
    }
}
