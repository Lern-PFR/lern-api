using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetaPoco;

namespace Lern_API.Helpers.Database
{
    public static class DatabaseExtension
    {
        private static ILogger _logger;

        public static IApplicationBuilder EnsureDatabase(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            _logger = scope.ServiceProvider.GetService<ILogger<Startup>>();
            var database = scope.ServiceProvider.GetService<IDatabase>();

            try
            {
                Retry.Do(() =>
                {
                    _logger.LogInformation("Tentative de connexion à la base de données...");

                    database.Execute("SELECT 1");

                    _logger.LogInformation("Connexion à la base de données réussie");
                }, TimeSpan.FromSeconds(5));
            }
            catch (AggregateException)
            {
                _logger.LogError("Les tentatives de connexion à la base de données ont échoué");
                throw;
            }

            return app;
        }
    }
}
