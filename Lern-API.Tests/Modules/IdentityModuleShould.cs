using System.Collections.Generic;
using System.Threading.Tasks;
using Lern_API.Models;
using Lern_API.Tests.Attributes;
using Lern_API.Utilities;
using Microsoft.Extensions.Configuration;
using Nancy;
using Nancy.Testing;
using Xunit;

namespace Lern_API.Tests.Modules
{
    public class IdentityModuleShould
    {
        [Fact]
        public async Task Return_Unauthorized_Without_Token()
        {
            var browser = new Browser(new LernBootstrapper());

            var result = await browser.Get("/me", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Unauthorized_With_Invalid_Token(string fakeToken)
        {
            var browser = new Browser(new LernBootstrapper(), d => d.Header("Authorization", $"Bearer {fakeToken}"));

            var result = await browser.Get("/me", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Unauthorized_With_Invalid_Header(string fakeHeader)
        {
            var browser = new Browser(new LernBootstrapper(), d => d.Header("Authorization", fakeHeader));

            var result = await browser.Get("/me", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Identity_With_Valid_Token(string name, string secret)
        {
            Configuration.Config = new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("SecretKey", secret)
            }).Build();

            var user = new User
            {
                AuthenticationType = "Test",
                IsAuthenticated = true,
                Name = name
            };

            var token = JwtHelper.Encode(user, secret);

            var browser = new Browser(new LernBootstrapper(), d => d.Header("Authorization", $"Bearer {token}"));

            var result = await browser.Get("/me", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(name, result.Body.AsString());
        }
    }
}
