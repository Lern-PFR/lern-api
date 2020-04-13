using System.Security.Principal;
using JWT.Algorithms;
using JWT.Builder;

namespace Lern_API.Utilities
{
    public static class Jwt
    {
        private static IJwtAlgorithm algorithm { get; } = new HMACSHA256Algorithm();

        public static T Decode<T>(string token) where T : IIdentity
        {
            return new JwtBuilder()
                .WithAlgorithm(algorithm)
                .WithSecret(Configuration.GetString("SecretKey"))
                .MustVerifySignature()
                .Decode<T>(token);
        }

        public static string Encode(IIdentity identity)
        {
            return new JwtBuilder()
                .WithAlgorithm(algorithm)
                .WithSecret(Configuration.GetString("SecretKey"))
                .AddClaims(identity.ToDictionary())
                .Encode();
        }
    }
}
