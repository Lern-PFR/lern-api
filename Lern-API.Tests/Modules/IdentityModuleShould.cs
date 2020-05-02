using System;
using System.Threading.Tasks;
using Lern_API.Models;
using Lern_API.Tests.Attributes;
using Lern_API.Utilities;
using Nancy;
using Nancy.Testing;
using PetaPoco;
using Xunit;
using Xunit.Abstractions;

namespace Lern_API.Tests.Modules
{
    public class IdentityModuleShould
    {
        private readonly ITestOutputHelper _output;

        public IdentityModuleShould(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Unauthorized_Without_Token(ILogger logger, IDatabase database)
        {
            var browser = new Browser(new LernBootstrapper(logger, database));

            var result = await browser.Get("/me", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Unauthorized_With_Invalid_Token(ILogger logger, IDatabase database, string fakeToken)
        {
            var browser = new Browser(new LernBootstrapper(logger, database), d => d.Header("Authorization", $"Bearer {fakeToken}"));

            var result = await browser.Get("/me", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Unauthorized_With_Invalid_Header(ILogger logger, IDatabase database, string fakeHeader)
        {
            var browser = new Browser(new LernBootstrapper(logger, database), d => d.Header("Authorization", fakeHeader));

            var result = await browser.Get("/me", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Theory(Skip = "Ne fonctionne pas en environnement de test Windows, le code testé fonctionne dans le même environnement")]
        [AutoMoqData]
        public async Task Return_Identity_With_Valid_Token(ILogger logger, IDatabase database, string name, string secret)
        {
            Environment.SetEnvironmentVariable("SECRET_KEY", secret);

            var user = new Identity
            {
                AuthenticationType = "Test",
                IsAuthenticated = true,
                Name = name
            };

            var token = JwtHelper.Encode(user, secret);

            var browser = new Browser(new LernBootstrapper(logger, database), d => d.Header("Authorization", $"Bearer {token}"));

            var result = await browser.Get("/me", with =>
            {
                with.HttpRequest();
            });

            _output.WriteLine($"Secret = {secret}");
            _output.WriteLine($"Token = {token}");

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(name, result.Body.AsString());
        }
    }
}
