using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lern_API.Utilities
{
    public static class ObjectToDictionaryHelper
    {
        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "Impossible de convertir l'objet source en dictionnaire : l'objet source est null.");

            var dictionary = new Dictionary<string, T>();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                var value = property.GetValue(source);

                if (value is T val)
                {
                    dictionary.Add(property.Name, val);
                }
            }

            return dictionary;
        }

    }
}
