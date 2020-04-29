using System;
using System.IO;
using System.Text;
using Lern_API.Tests.Attributes;
using Lern_API.Utilities;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Lern_API.Tests.Utilities
{
    public class ConfigurationShould
    {
        [Theory]
        [AutoMoqData]
        public void Throw_On_Invalid_Key(IConfiguration configuration)
        {
            Configuration.Config = configuration;

            Assert.Throws<ArgumentNullException>(() => Configuration.Get<string>(null));
            Assert.Throws<ArgumentNullException>(() => Configuration.GetList(null));

            Assert.Throws<ArgumentException>(() => Configuration.Get<string>(string.Empty));
            Assert.Throws<ArgumentException>(() => Configuration.GetList(string.Empty));

            Assert.Throws<ArgumentException>(() => Configuration.Get<string>(" "));
            Assert.Throws<ArgumentException>(() => Configuration.GetList(" "));
        }

        [Theory]
        [AutoMoqData]
        public void Return_Empty_Value_If_Key_Not_Found(IConfiguration configuration, string key)
        {
            Configuration.Config = configuration;

            Assert.Null(Configuration.Get<string>(key));
            Assert.Equal(0, Configuration.Get<int>(key));
            Assert.Equal(0.0d, Configuration.Get<double>(key));

            var list = Configuration.GetList(key);
            Assert.NotNull(list);
            Assert.Empty(list);
        }

        [Theory]
        [AutoMoqData]
        public void Return_List_From_Configuration(Mock<IConfiguration> configuration, string key)
        {
            var configSection = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes("{\"Array\": [\"valeur1\",\"valeur2\"]}"))).Build();

            configuration.Setup(c => c.GetSection(key)).Returns(configSection.GetSection("Array"));

            Configuration.Config = configuration.Object;

            var list = Configuration.GetList(key);

            Assert.NotNull(list);
            Assert.Collection(list, e => Assert.Equal("valeur1", e),
                e => Assert.Equal("valeur2", e));
        }
    }
}
