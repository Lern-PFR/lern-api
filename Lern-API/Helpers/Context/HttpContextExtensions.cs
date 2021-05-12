using Microsoft.AspNetCore.Http;

namespace Lern_API.Helpers.Context
{
    public static class HttpContextExtensions
    {
        public static string GetBaseUrl(this HttpContext context) => $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";
    }
}
