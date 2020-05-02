using System.Diagnostics.CodeAnalysis;
using FluentMigrator;

namespace Lern_API.Migrations
{
    [ExcludeFromCodeCoverage]
    [Migration(2020_05_02)]
    public class AddUserTable : Migration
    {
        public override void Up()
        {
            Create.Table("users")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("CreatedOn").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("Name").AsString().NotNullable().Unique();
        }

        public override void Down()
        {
            Delete.Table("users");
        }
    }
}
