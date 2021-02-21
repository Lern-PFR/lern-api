using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lern_API.Helpers.AspNet
{
    [ExcludeFromCodeCoverage]
    public static class GetColumnsExtension
    {
        public static async Task<IEnumerable<string>> GetColumns(this HttpContext context)
        {
            if (context == null)
                return null;

            if (!context.Request.Body.CanSeek)
                throw new InvalidOperationException($"Veuillez ajouter l'attribut {nameof(EnableBodyRewindAttribute)} à votre méthode afin d'utiliser {nameof(GetColumns)}");

            context.Request.Body.Position = 0;

            Dictionary<string, object> dictionary;

            try
            {
                dictionary = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(context.Request.Body);
            }
            catch (JsonException)
            {
                dictionary = new Dictionary<string, object>();
            }

            return dictionary?.Keys.AsEnumerable();
        }
    }
}
