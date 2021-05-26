using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Lern_API
{
    public class LernContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Concept> Concepts { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Progression> Progressions { get; set; }
        public DbSet<Purchase> Purchases { get; set; }

        public LernContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Nickname).IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email).IsUnique();

            modelBuilder.Entity<Subject>()
                .HasMany(x => x.Modules)
                .WithOne().HasForeignKey(x => x.SubjectId)
                .IsRequired();

            modelBuilder.Entity<Module>()
                .HasMany(x => x.Concepts)
                .WithOne().HasForeignKey(x => x.ModuleId)
                .IsRequired();

            modelBuilder.Entity<Concept>()
                .HasMany(x => x.Courses)
                .WithOne().HasForeignKey(x => x.ConceptId)
                .IsRequired();
            modelBuilder.Entity<Concept>()
                .HasMany(x => x.Exercises)
                .WithOne().HasForeignKey(x => x.ConceptId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Course>()
                .HasKey(x => new { x.Id, x.Version });
            modelBuilder.Entity<Course>()
                .HasMany(x => x.Exercises)
                .WithOne().HasForeignKey(x => new { x.CourseId, x.CourseVersion })
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasMany(x => x.Answers)
                .WithOne().HasForeignKey(x => x.QuestionId)
                .IsRequired();

            modelBuilder.Entity<Result>()
                .HasKey(x => new { x.QuestionId, x.UserId });

            modelBuilder.Entity<Progression>()
                .HasKey(x => new { x.UserId, x.SubjectId });

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            AddTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void AddTimestamps()
        {
            var now = DateTime.UtcNow;

            var entities = ChangeTracker.Entries()
                .Where(x => x.State is EntityState.Added or EntityState.Modified);
            
            foreach (var entry in entities)
            {
                var createdAt = entry.Entity.GetType().GetProperty("CreatedAt");
                var updatedAt = entry.Entity.GetType().GetProperty("UpdatedAt");

                if (createdAt != null && entry.State == EntityState.Added)
                    createdAt.SetValue(entry.Entity, now);

                if (updatedAt != null)
                    updatedAt.SetValue(entry.Entity, now);
            }
        }
    }
}
