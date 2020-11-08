using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using FluentMigrator;
using FluentMigrator.Builders;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Runner;
using Lern_API.Models;
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
                .WithCreatedAt()
                .WithUpdatedAt();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax Table<T>(this ICreateExpressionRoot create) where T : AbstractModel
        {
            return create.Table(Inflector.Table<T>()).InheritAbstractModel();
        }

        public static IInSchemaSyntax Table<T>(this IDeleteExpressionRoot delete) where T : AbstractModel
        {
            return delete.Table(Inflector.Table<T>());
        }

        public static ICreateTableColumnAsTypeSyntax WithColumn(this ICreateTableWithColumnSyntax table, Expression<Func<object>> prop)
        {
            return table.WithColumn(Inflector.Column(prop));
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax WithId(this ICreateTableWithColumnSyntax table)
        {
            return table.WithColumn(Inflector.Column(nameof(AbstractModel.Id))).AsGuid().NotNullable().PrimaryKey();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax WithCreatedAt(this ICreateTableWithColumnSyntax table)
        {
            return table
                .WithColumn(Inflector.Column(nameof(AbstractModel.CreatedAt))).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax WithUpdatedAt(this ICreateTableWithColumnSyntax table)
        {
            return table
                .WithColumn(Inflector.Column(nameof(AbstractModel.UpdatedAt))).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
        }

        public static ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ForeignKey<T>(this IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax, ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax> column, Expression<Func<object>> prop) where T : AbstractModel
        {
            return column.ForeignKey(Inflector.Table<T>(), Inflector.Column(prop));
        }
    }
}
