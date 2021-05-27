using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lern_API.Helpers.Models
{
    public static class ModelsExtensions
    {
        public static void CloneFrom<TModel, TOrigin>(this TModel model, TOrigin origin)
        {
            foreach (var prop in typeof(TOrigin).GetProperties())
            {
                if (IsList(prop.PropertyType))
                    continue;

                var destProp = model.GetType().GetProperty(prop.Name);

                if (destProp == null || destProp.PropertyType != prop.PropertyType)
                    continue;

                destProp.SetValue(model, prop.GetValue(origin));
            }
        }

        /// <summary>
        /// Indicates whether or not the specified type is a list.
        /// </summary>
        /// <param name="type">The type to query</param>
        /// <returns>True if the type is a list, otherwise false</returns>
        private static bool IsList(Type type)
        {
            if (type == null)
                return false;

            return typeof(IList).IsAssignableFrom(type) || type.GetInterfaces().Any(it => it.IsGenericType && typeof(IList<>) == it.GetGenericTypeDefinition());
        }
    }
}
