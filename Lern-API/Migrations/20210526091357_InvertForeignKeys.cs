using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lern_API.Migrations
{
    public partial class InvertForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Concepts_ConceptId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Courses_CourseId1_CourseVersion",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_CourseId1_CourseVersion",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "Exercises");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CourseId_CourseVersion",
                table: "Exercises",
                columns: new[] { "CourseId", "CourseVersion" });

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Concepts_ConceptId",
                table: "Exercises",
                column: "ConceptId",
                principalTable: "Concepts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Courses_CourseId_CourseVersion",
                table: "Exercises",
                columns: new[] { "CourseId", "CourseVersion" },
                principalTable: "Courses",
                principalColumns: new[] { "Id", "Version" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Concepts_ConceptId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Courses_CourseId_CourseVersion",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_CourseId_CourseVersion",
                table: "Exercises");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId1",
                table: "Exercises",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CourseId1_CourseVersion",
                table: "Exercises",
                columns: new[] { "CourseId1", "CourseVersion" });

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Concepts_ConceptId",
                table: "Exercises",
                column: "ConceptId",
                principalTable: "Concepts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Courses_CourseId1_CourseVersion",
                table: "Exercises",
                columns: new[] { "CourseId1", "CourseVersion" },
                principalTable: "Courses",
                principalColumns: new[] { "Id", "Version" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
