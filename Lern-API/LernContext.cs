using System;
using System.Linq;
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
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Nickname).IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email).IsUnique();

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

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => x.State is EntityState.Added or EntityState.Modified);

            foreach (var entity in entities)
            {
                var now = DateTime.UtcNow;
                var createdAt = entity.Entity.GetType().GetProperty("CreatedAt");
                var updatedAt = entity.Entity.GetType().GetProperty("UpdatedAt");

                if (entity.State == EntityState.Added && createdAt != null)
                    createdAt.SetValue(entity.Entity, now);

                if (updatedAt != null)
                    updatedAt.SetValue(entity.Entity, now);
            }
        }
    }
}
