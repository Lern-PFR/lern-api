using System;
using System.Collections.Generic;
using Lern_API.Helpers;
using Lern_API.Tests.Attributes;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Lern_API.Tests.Helpers
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
        public void Return_Value_From_Configuration(string key, string value)
        {
            Configuration.Config = new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>(key, value)
            }).Build();

            var result = Configuration.Get<string>(key);

            Assert.Equal(value, result);
        }

        [Theory]
        [AutoMoqData]
        public void Return_List_From_Configuration(Mock<IConfiguration> configuration, string key)
        {
            var configSection = new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("Array:0", "valeur1"),
                new KeyValuePair<string, string>("Array:1", "valeur2")
            }).Build();

            configuration.Setup(c => c.GetSection(key)).Returns(configSection.GetSection("Array"));

            Configuration.Config = configuration.Object;

            var list = Configuration.GetList(key);

            Assert.NotNull(list);
            Assert.Collection(list, e => Assert.Equal("valeur1", e),
                e => Assert.Equal("valeur2", e));
        }

        [Theory]
        [AutoMoqData]
        public void Use_Env_Var_Before_Config_File(string value, string list1, string list2)
        {
            Environment.SetEnvironmentVariable("ENV_VAR", value);
            Environment.SetEnvironmentVariable("ENV_LIST", $"{list1};{list2}");

            var result = Configuration.Get<string>("EnvVar");
            var list = Configuration.GetList("EnvList");

            Assert.Equal(value, result);
            Assert.Collection(list, e => Assert.Equal(list1, e),
                e => Assert.Equal(list2, e));
        }

        [Theory]
        [AutoMoqData]
        public void Build_Valid_Connection_String(string host, string username, string password, string database, int port)
        {
            Environment.SetEnvironmentVariable("DB_HOST", host);
            Environment.SetEnvironmentVariable("DB_USERNAME", username);
            Environment.SetEnvironmentVariable("DB_PASSWORD", password);
            Environment.SetEnvironmentVariable("DB_DATABASE", database);
            Environment.SetEnvironmentVariable("DB_PORT", port.ToString());

            var result = Configuration.GetConnectionString().Split(';');

            Assert.Contains(result, e => e.ToLower().Equals($"host={host}"));
            Assert.Contains(result, e => e.ToLower().Equals($"username={username}"));
            Assert.Contains(result, e => e.ToLower().Equals($"password={password}"));
            Assert.Contains(result, e => e.ToLower().Equals($"database={database}"));
            Assert.Contains(result, e => e.ToLower().Equals($"port={port}"));
        }
    }
}
