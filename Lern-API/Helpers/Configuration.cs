using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Lern_API.Helpers
{
    public static class Configuration
    {
        public static IConfiguration Config { get; set; } = new ConfigurationBuilder().Build();

        private static string CamelToUpperSnake(string str) => string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToUpperInvariant();

        public static T Get<T>(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            var env = Environment.GetEnvironmentVariable(CamelToUpperSnake(key));

            if (!string.IsNullOrEmpty(env))
                return (T)Convert.ChangeType(env, typeof(T));

            return Config.GetValue<T>(key);
        }

        public static IEnumerable<string> GetList(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            
            var env = Environment.GetEnvironmentVariable(CamelToUpperSnake(key));

            if (!string.IsNullOrEmpty(env))
                return env.Split(';');

            return Config.GetSection(key).GetChildren().Select(c => c.Value);
        }

        public static string GetConnectionString()
        {
            var host = Get<string>("DbHost");
            var username = Get<string>("DbUsername");
            var password = Get<string>("DbPassword");
            var db = Get<string>("DbDatabase");
            var port = Get<int>("DbPort");

            return $"Host={host};Username={username};password={password};database={db};port={port}";
        }
    }
}
