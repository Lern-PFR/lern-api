using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;
using Lern_API.Tests.Attributes;
using Lern_API.Utilities;
using Xunit;

namespace Lern_API.Tests.Utilities
{
    [ExcludeFromCodeCoverage]
    public class JwtHelperShould
    {
        [Theory]
        [AutoMoqData]
        public void Throw_On_Invalid_Parameters(IIdentity identity, string token, string secret)
        {
            Assert.Throws<ArgumentNullException>(() => JwtHelper.Encode(null, secret));
            Assert.Throws<ArgumentNullException>(() => JwtHelper.Encode(identity, null));

            Assert.Throws<ArgumentNullException>(() => JwtHelper.Decode<ClaimsIdentity>(null, secret));
            Assert.Throws<ArgumentNullException>(() => JwtHelper.Decode<ClaimsIdentity>(token, null));

            Assert.Throws<ArgumentException>(() => JwtHelper.Encode(identity, " "));

            Assert.Throws<ArgumentException>(() => JwtHelper.Decode<ClaimsIdentity>(" ", secret));
            Assert.Throws<ArgumentException>(() => JwtHelper.Decode<ClaimsIdentity>(token, " "));
        }

        [Theory]
        [AutoMoqData]
        public void Encode_Identity(string secret)
        {
            var identity = new ClaimsIdentity();
            var result = JwtHelper.Encode(identity, secret);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Theory]
        [AutoMoqData]
        public void Throw_On_Invalid_Token(string token, string secret)
        {
            Assert.ThrowsAny<Exception>(() => JwtHelper.Decode<ClaimsIdentity>(token, secret));
        }

        [Theory]
        [AutoMoqData]
        public void Encode_And_Decode(string secret)
        {
            var user = new TestUser
            {
                Name = "value"
            };

            var token = JwtHelper.Encode(user, secret);

            var result = JwtHelper.Decode<TestUser>(token, secret);

            Assert.NotNull(result);
            Assert.NotNull(result.Name);
            Assert.Equal("value", result.Name);
        }

        private class TestUser : IIdentity
        {
            public string AuthenticationType { get; } = "Test";
            public bool IsAuthenticated { get; } = true;
            public string Name { get; set; }
        }
    }
}
