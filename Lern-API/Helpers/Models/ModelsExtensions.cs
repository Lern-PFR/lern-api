using System;

namespace Lern_API.Helpers.Models
{
    public static class ModelsExtensions
    {
        public static void CloneFrom<TModel, TOrigin>(this TModel model, TOrigin origin)
        {
            foreach (var prop in typeof(TOrigin).GetProperties())
            {
                #if DEBUG

                var destProp = model.GetType().GetProperty(prop.Name);

                if (destProp != null && destProp.GetType() != prop.GetType())
                {
                    throw new ArgumentException($"Cannot clone model : cloning origin ({typeof(TOrigin)}) is not compatible with destination ({typeof(TModel)})", prop.Name);
                }
                
                #endif

                typeof(TModel).GetProperty(prop.Name)?.SetValue(model, prop.GetValue(origin));
            }
        }
    }
}
