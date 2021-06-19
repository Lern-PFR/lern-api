using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lern_API.Migrations
{
    public partial class RenameLesson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Courses_CourseId_CourseVersion",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "CourseVersion",
                table: "Exercises",
                newName: "LessonVersion");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Exercises",
                newName: "LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_Exercises_CourseId_CourseVersion",
                table: "Exercises",
                newName: "IX_Exercises_LessonId_LessonVersion");

            migrationBuilder.RenameTable(
                name: "Courses",
                newName: "Lessons");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_ConceptId",
                table: "Lessons",
                column: "ConceptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Lessons_LessonId_LessonVersion",
                table: "Exercises",
                columns: new[] { "LessonId", "LessonVersion" },
                principalTable: "Lessons",
                principalColumns: new[] { "Id", "Version" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Lessons_LessonId_LessonVersion",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "LessonVersion",
                table: "Exercises",
                newName: "CourseVersion");

            migrationBuilder.RenameColumn(
                name: "LessonId",
                table: "Exercises",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Exercises_LessonId_LessonVersion",
                table: "Exercises",
                newName: "IX_Exercises_CourseId_CourseVersion");

            migrationBuilder.RenameTable(
                name: "Lessons",
                newName: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ConceptId",
                table: "Courses",
                column: "ConceptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Courses_CourseId_CourseVersion",
                table: "Exercises",
                columns: new[] { "CourseId", "CourseVersion" },
                principalTable: "Courses",
                principalColumns: new[] { "Id", "Version" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
