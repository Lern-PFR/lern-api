using FluentMigrator;

namespace Lern_API.Migrations
{
    [Migration(2020_10_09)]
    public class AddUserTable : Migration
    {
        public override void Up()
        {
            Create.Table("users")
                .WithColumn("Id").AsGuid().PrimaryKey().WithDefault(SystemMethods.NewGuid)
                .WithColumn("CreatedOn").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("Name").AsString(50).NotNullable().Unique()
                .WithColumn("Email").AsString().NotNullable().Unique()
                .WithColumn("Password").AsString().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("users");
        }
    }
}
