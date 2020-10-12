using System;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lern_API.Helpers.Swagger
{
    public class ReadOnlyAttribute : Attribute
    {

    }

    public class ReadOnlyPropertiesFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.MemberInfo?.GetCustomAttributes(typeof(ReadOnlyAttribute), true).SingleOrDefault() == null)
                return;

            schema.ReadOnly = true;
            schema.Deprecated = true;

            if (schema.Properties == null)
                return;
            
            foreach (var keyValuePair in schema.Properties)
            {
                keyValuePair.Value.ReadOnly = false;
            }
        }
    }
}
