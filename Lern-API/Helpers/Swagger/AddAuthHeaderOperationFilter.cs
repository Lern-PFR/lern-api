using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lern_API.Helpers.JWT;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lern_API.Helpers.Swagger
{
    [ExcludeFromCodeCoverage]
    public class AddAuthHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.GetCustomAttributes(typeof(RequireAuthenticationAttribute), true).SingleOrDefault() == null)
                return;

            operation.Security ??= new List<OpenApiSecurityRequirement>();

            var scheme = new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearer" } };

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [scheme] = new List<string>()
            });
        }
    }
}
