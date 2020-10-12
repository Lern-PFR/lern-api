namespace Lern_API.Helpers.Models
{
    public static class ModelsExtensions
    {
        public static void CloneFrom<T>(this T model, T origin)
        {
            foreach (var prop in origin.GetType().GetProperties())
            {
                origin.GetType().GetProperty(prop.Name)?.SetValue(model, prop.GetValue(origin));
            }
        }
    }
}
