using System.Diagnostics.CodeAnalysis;
using FluentMigrator;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lern_API.Helpers.Database
{
    [ExcludeFromCodeCoverage]
    public static class MigrationExtensions
    {
        private static ILogger _logger;

        public static IApplicationBuilder Migrate(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            _logger = scope.ServiceProvider.GetService<ILogger<Startup>>();
            var runner = scope.ServiceProvider.GetService<IMigrationRunner>();

            if (!runner.HasMigrationsToApplyUp())
            {
                _logger.LogInformation("Aucune migration à effectuer, la base de données est à jour");
            }
            else
            {
                _logger.LogInformation("Migration de la base de données vers une nouvelle version...");

                runner.MigrateUp();

                _logger.LogInformation("Migration terminée");
            }

            return app;
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax InheritAbstractModel(this ICreateTableWithColumnSyntax table)
        {
            return table
                .WithId()
                .WithCreatedAt();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax WithId(this ICreateTableWithColumnSyntax table)
        {
            return table.WithColumn("id").AsGuid().NotNullable().PrimaryKey();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax WithCreatedAt(this ICreateTableWithColumnSyntax table)
        {
            return table
                .WithColumn("createdAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
        }
    }
}
