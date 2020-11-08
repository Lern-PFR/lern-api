using System.Diagnostics.CodeAnalysis;
using FluentMigrator;
using Lern_API.Helpers.Database;
using Lern_API.Models;

namespace Lern_API.Migrations
{
    [ExcludeFromCodeCoverage]
    [Migration(1)]
    public class InitializeDatabase : Migration
    {
#pragma warning disable S3459 // Unassigned members should be removed
#pragma warning disable 649
        private readonly User _user;
        private readonly Subject _subject;
        private readonly Module _module;
        private readonly Concept _concept;
        private readonly Course _course;
#pragma warning restore 649
#pragma warning restore S3459 // Unassigned members should be removed

        public override void Up()
        {
            Create.Table<User>()
                .WithColumn(() => _user.ManagerId).AsGuid().Nullable()
                    .ForeignKey<User>(() => _user.Id)
                .WithColumn(() => _user.Firstname).AsString(50).NotNullable()
                .WithColumn(() => _user.Lastname).AsString(100).NotNullable()
                .WithColumn(() => _user.Nickname).AsString(50).NotNullable()
                .WithColumn(() => _user.Email).AsString(254).NotNullable().Unique()
                .WithColumn(() => _user.Password).AsString().NotNullable()
                .WithColumn(() => _user.Tokens).AsInt32().WithDefaultValue(0)
                .WithColumn(() => _user.MaxSubjects).AsInt32().WithDefaultValue(5)
                .WithColumn(() => _user.Active).AsBoolean().WithDefaultValue(true)
                .WithColumn(() => _user.Admin).AsBoolean().WithDefaultValue(false)
                .WithColumn(() => _user.VerifiedCreator).AsBoolean().WithDefaultValue(false);

            Create.Table<Subject>()
                .WithColumn(() => _subject.Author).AsGuid().NotNullable()
                    .ForeignKey<User>(() => _subject.Id)
                .WithColumn(() => _subject.Title).AsString(50).NotNullable()
                .WithColumn(() => _subject.Description).AsString(300).NotNullable()
                .WithColumn(() => _subject.State).AsInt16().NotNullable().WithDefaultValue((int) SubjectState.Pending);

            Create.Table<Module>()
                .WithColumn(() => _module.SubjectId).AsGuid().NotNullable()
                    .ForeignKey<Subject>(() => _subject.Id)
                .WithColumn(() => _module.Title).AsString(50).NotNullable()
                .WithColumn(() => _module.Description).AsString(300).NotNullable()
                .WithColumn(() => _module.Order).AsInt32().NotNullable();

            Create.Table<Concept>()
                .WithColumn(() => _concept.ModuleId).AsGuid().NotNullable()
                    .ForeignKey<Module>(() => _module.Id)
                .WithColumn(() => _concept.Title).AsString(50).NotNullable()
                .WithColumn(() => _concept.Description).AsString(300).NotNullable()
                .WithColumn(() => _concept.Order).AsInt32().NotNullable();

            Create.Table<Course>()
                .WithColumn(() => _course.Version).AsInt32().NotNullable().PrimaryKey()
                .WithColumn(() => _course.ConceptId).AsGuid().NotNullable()
                    .ForeignKey<Concept>(() => _concept.Id)
                .WithColumn(() => _course.Title).AsString(50).NotNullable()
                .WithColumn(() => _course.Description).AsString(300).NotNullable()
                .WithColumn(() => _course.Content).AsString().NotNullable()
                .WithColumn(() => _course.Order).AsInt32().NotNullable();
        }

        public override void Down()
        {
            Delete.Table<User>();
            Delete.Table<Subject>();
            Delete.Table<Module>();
            Delete.Table<Concept>();
            Delete.Table<Course>();
        }
    }
}
