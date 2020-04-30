using System;
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
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException($"{nameof(token)} est vide");

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException($"{nameof(secretKey)} est vide");

            return new JwtBuilder()
                .WithAlgorithm(Algorithm)
                .WithSecret(secretKey)
                .MustVerifySignature()
                .Decode<T>(token);
        }

        public static string Encode(IIdentity identity, string secretKey)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException($"{nameof(secretKey)} est vide");

            return new JwtBuilder()
                .WithAlgorithm(Algorithm)
                .WithSecret(secretKey)
                .AddClaims(identity.ToDictionary())
                .Encode();
        }
    }
}
