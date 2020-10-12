using System.Diagnostics.CodeAnalysis;
using FluentMigrator;

namespace Lern_API.Migrations
{
    [ExcludeFromCodeCoverage]
    [Migration(1)]
    public class InitializeDatabase : Migration
    {
        public override void Up()
        {
            Create.Table("users")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("createdAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("name").AsString(50).NotNullable().Unique()
                .WithColumn("email").AsString().NotNullable().Unique()
                .WithColumn("password").AsString().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("users");
        }
    }
}
