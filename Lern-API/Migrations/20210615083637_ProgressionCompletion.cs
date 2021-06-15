using Microsoft.EntityFrameworkCore.Migrations;

namespace Lern_API.Migrations
{
    public partial class ProgressionCompletion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Completed",
                table: "Progressions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Completion",
                table: "Progressions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Completed",
                table: "Progressions");

            migrationBuilder.DropColumn(
                name: "Completion",
                table: "Progressions");
        }
    }
}
