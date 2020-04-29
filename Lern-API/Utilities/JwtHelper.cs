using System.Security.Principal;
using JWT.Algorithms;
using JWT.Builder;

namespace Lern_API.Utilities
{
    public static class JwtHelper
    {
        private static IJwtAlgorithm Algorithm { get; } = new HMACSHA256Algorithm();

        public static T Decode<T>(string token, string secretKey) where T : IIdentity, new()
        {
            return new JwtBuilder()
                .WithAlgorithm(Algorithm)
                .WithSecret(secretKey)
                .MustVerifySignature()
                .Decode<T>(token);
        }

        public static string Encode(IIdentity identity, string secretKey)
        {
            return new JwtBuilder()
                .WithAlgorithm(Algorithm)
                .WithSecret(secretKey)
                .AddClaims(identity.ToDictionary())
                .Encode();
        }
    }
}
