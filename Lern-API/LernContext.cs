using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.Models;
using Microsoft.EntityFrameworkCore;
using Module = Lern_API.Models.Module;

namespace Lern_API
{
    public class LernContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Concept> Concepts { get; set; }
        public DbSet<Course> Courses { get; set; }

        public LernContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>()
                .HasKey(x => new { x.Id, x.Version });

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

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            AddTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => 
                        x.Entity.GetType().GetProperties(BindingFlags.Public)
                            .Select(prop => prop.Name)
                            .Any(propName => propName.Contains("CreatedAt") || propName.Contains("UpdatedAt")) &&
                        (x.State == EntityState.Added || x.State == EntityState.Modified)
                );

            foreach (var entity in entities)
            {
                var now = DateTime.UtcNow;
                var createdAt = entity.GetType().GetProperty("CreatedAt");
                var updatedAt = entity.GetType().GetProperty("UpdatedAt");

                if (entity.State == EntityState.Added && createdAt != null)
                    createdAt.SetValue(entity.Entity, now);

                if (updatedAt != null)
                    updatedAt.SetValue(entity.Entity, now);
            }
        }
    }
}
