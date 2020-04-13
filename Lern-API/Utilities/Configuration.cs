using System.Collections.Generic;

namespace Lern_API.Utilities
{
    public static class Configuration
    {
        private static Logger Log { get; } = Logger.GetLogger(typeof(Configuration));

        public static string GetString(string key, bool errorOnEmpty = true)
        {
            //var value = ConfigurationManager.AppSettings[key];

            var value = "";

            if (!errorOnEmpty || !string.IsNullOrWhiteSpace(value))
                return value;
            
            Log.Error($"{key} n'est pas renseigné !");

            return null;
        }

        public static int GetInt(string key, bool errorOnEmpty = true)
        {
            var value = GetString(key, errorOnEmpty);

            if (value == null)
            {
                Log.Error($"{key} n'est pas un nombre entier ! Utilisation de -1 par défaut");
                return -1;
            }

            var success = int.TryParse(value, out var number);

            if (success || !errorOnEmpty)
                return number;

            return number;
        }

        public static IEnumerable<string> GetList(string key, bool errorOnEmpty = true)
        {
            var value = GetString(key, errorOnEmpty);

            return value?.Split(',');
        }
    }
}
