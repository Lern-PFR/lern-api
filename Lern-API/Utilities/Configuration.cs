using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NullGuard;

namespace Lern_API.Utilities
{
    public static class Configuration
    {
        public static IConfiguration Config { get; set; } = new ConfigurationBuilder().Build();

        [return: AllowNull]
        public static T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));

            return Config.GetValue<T>(key);
        }

        [return: AllowNull]
        public static IEnumerable<string> GetList(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            
            return Config.GetSection(key).GetChildren().Select(c => c.Value).ToArray();
        }
    }
}
