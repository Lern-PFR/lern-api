using System.Diagnostics.CodeAnalysis;
using FluentMigrator;
using Lern_API.Helpers.Database;

namespace Lern_API.Migrations
{
    [ExcludeFromCodeCoverage]
    [Migration(1)]
    public class InitializeDatabase : Migration
    {
        public override void Up()
        {
            Create.Table("users")
                .InheritAbstractModel()
                .WithColumn("manager").AsGuid().Nullable()
                    .ForeignKey("users", "id")
                .WithColumn("firstname").AsString(50).NotNullable()
                .WithColumn("lastname").AsString(100).NotNullable()
                .WithColumn("nickname").AsString(50).NotNullable()
                .WithColumn("email").AsString(254).NotNullable().Unique()
                .WithColumn("password").AsString().NotNullable()
                .WithColumn("tokens").AsInt32().WithDefaultValue(0)
                .WithColumn("maxTopics").AsInt32().WithDefaultValue(5)
                .WithColumn("active").AsBoolean().WithDefaultValue(true)
                .WithColumn("admin").AsBoolean().WithDefaultValue(false)
                .WithColumn("verifiedCreator").AsBoolean().WithDefaultValue(false);

            Create.Table("subject")
                .InheritAbstractModel();
        }

        public override void Down()
        {
            Delete.Table("users");
        }
    }
}
