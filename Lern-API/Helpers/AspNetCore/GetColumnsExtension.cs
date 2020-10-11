using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lern_API.Helpers.AspNetCore
{
    public static class GetColumnsExtension
    {
        public static async Task<IEnumerable<string>> GetColumns(this HttpContext context)
        {
            if (!context.Request.Body.CanSeek)
                throw new InvalidOperationException($"Veuillez ajouter l'attribut {nameof(EnableBodyRewindAttribute)} à votre méthode afin d'utiliser {nameof(GetColumns)}");

            context.Request.Body.Position = 0;
            var dictionary = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(context.Request.Body);
            return dictionary.Keys.AsEnumerable();
        }
    }
}
