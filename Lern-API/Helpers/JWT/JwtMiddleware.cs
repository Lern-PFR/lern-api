using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lern_API.Services;
using Lern_API.Services.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Lern_API.Helpers.JWT
{
    [ExcludeFromCodeCoverage]
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                await AttachUserToContext(context, userService, token);

            await _next(context);
        }

        private static async Task AttachUserToContext(HttpContext context, IUserService userService, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(Configuration.Get<string>("SecretKey"));

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwtToken = (JwtSecurityToken) validatedToken;
                var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "Id").Value);

                context.Items["User"] = await userService.Get(userId);
            }
            catch
            {
                // ignored
            }
        }
    }
}
