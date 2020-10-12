using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lern_API.Helpers.AspNetCore
{
    [ExcludeFromCodeCoverage]
    public class EnableBodyRewindAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var req = context.HttpContext.Request;

            // Allows using several time the stream in ASP.Net Core
            req.EnableBuffering();
        }
    }
}
