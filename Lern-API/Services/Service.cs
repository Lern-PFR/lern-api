using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.Helpers.Models;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services
{
    public interface IService<TEntity, in TDataTransferObject> where TEntity : class, new()
    {
        Task<IEnumerable<TEntity>> GetAll(CancellationToken token = default);
        Task<TEntity> Get(Guid id, CancellationToken token = default);
        Task<TEntity> Create(TDataTransferObject entity, CancellationToken token = default);
        Task<TEntity> Update(Guid id, TDataTransferObject entity, CancellationToken token = default);
        Task<TEntity> Update(TEntity entity, CancellationToken token = default);
        Task<bool> Delete(Guid id, CancellationToken token = default);
        Task<bool> Exists(Guid id, CancellationToken token = default);
    }

    public class Service<TEntity, TDataTransferObject> : IService<TEntity, TDataTransferObject> where TEntity : class, new()
    {
        private LernContext Context { get; }
        private DbSet<TEntity> DbSet { get; }

        public Service(LernContext context)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
        }

        public virtual async Task<TEntity> Create(TDataTransferObject entity, CancellationToken token = default)
        {
            var final = new TEntity();
            final.CloneFrom(entity);

            var entityEntry = await SafeExecute(async () => await DbSet.AddAsync(final, token), token);

            return entityEntry?.Entity;
        }

        public virtual async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            var entity = await DbSet.FindAsync(new object[] { id }, token);

            if (entity == default)
                return false;

            var result = await SafeExecute(() => DbSet.Remove(entity), token);

            return !result.Equals(default);
        }

        public virtual async Task<bool> Exists(Guid id, CancellationToken token = default)
        {
            return await DbSet.FindAsync(new object[] { id }, token) != default;
        }

        public virtual async Task<TEntity> Get(Guid id, CancellationToken token = default)
        {
            return await DbSet.FindAsync(new object[] { id }, token);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAll(CancellationToken token = default)
        {
            return await DbSet.ToListAsync(token);
        }

        public virtual async Task<TEntity> Update(Guid id, TDataTransferObject entity, CancellationToken token = default)
        {
            var entry = await Get(id, token);
            
            entry.CloneFrom(entity);

            await SafeExecute(() => DbSet.Update(entry), token);

            return entry;
        }

        public virtual async Task<TEntity> Update(TEntity entity, CancellationToken token = default)
        {
            var result = await SafeExecute(() => DbSet.Update(entity), token);

            return result.Entity;
        }

        private async Task<T> SafeExecute<T>(Func<Task<T>> operations, CancellationToken token = default)
        {
            await using var transaction = await Context.Database.BeginTransactionAsync(token);

            try
            {
                var result = await operations();

                await Context.SaveChangesAsync(token);
                await transaction.CommitAsync(token);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync(token);
                return default;
            }
        }

        private async Task<T> SafeExecute<T>(Func<T> operations, CancellationToken token = default)
        {
            await using var transaction = await Context.Database.BeginTransactionAsync(token);

            try
            {
                var result = operations();

                await Context.SaveChangesAsync(token);
                await transaction.CommitAsync(token);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync(token);
                return default;
            }
        }
    }
}
