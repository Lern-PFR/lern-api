using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.Helpers.Models;
using Lern_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services
{
    public interface IService<TEntity, in TDataTransferObject> where TEntity : class, IModelBase, new()
    {
        Task<IEnumerable<TEntity>> GetAll(CancellationToken token = default);
        Task<TEntity> Get(Guid id, CancellationToken token = default);
        Task<TEntity> Create(TDataTransferObject entity, CancellationToken token = default);
        Task<TEntity> Update(Guid id, TDataTransferObject entity, CancellationToken token = default);
        Task<bool> Delete(Guid id, CancellationToken token = default);
        Task<bool> Exists(Guid id, CancellationToken token = default);
        Task<T> ExecuteTransaction<T>(Func<DbSet<TEntity>, Task<T>> operations, CancellationToken token = default);
        Task<T> ExecuteTransaction<T>(Func<DbSet<TEntity>, T> operations, CancellationToken token = default);
    }

    public class Service<TEntity, TDataTransferObject> : IService<TEntity, TDataTransferObject> where TEntity : class, IModelBase, new()
    {
        protected LernContext Context { get; }
        protected DbSet<TEntity> DbSet { get; }

        public Service(LernContext context)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
        }

        public virtual async Task<TEntity> Create(TDataTransferObject entity, CancellationToken token = default)
        {
            var final = new TEntity();
            final.CloneFrom(entity);

            var entityEntry = await SafeExecute(async set => await set.AddAsync(final, token), token);

            return entityEntry?.Entity;
        }

        public virtual async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            var exists = await Exists(id, token);

            if (!exists)
                return false;

            var result = await SafeExecute(async set => set.Remove(await Get(id, token)), token);

            return !result.Equals(default);
        }

        public virtual async Task<bool> Exists(Guid id, CancellationToken token = default)
        {
            return await DbSet.AnyAsync(x => x.Id == id, token);
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

            var result = await SafeExecute(set => set.Update(entry), token);

            return result?.Entity;
        }

        public async Task<T> ExecuteTransaction<T>(Func<DbSet<TEntity>, Task<T>> operations, CancellationToken token = default)
        {
            return await SafeExecute(operations, token);
        }

        public async Task<T> ExecuteTransaction<T>(Func<DbSet<TEntity>, T> operations, CancellationToken token = default)
        {
            return await SafeExecute(operations, token);
        }

        protected async Task<T> SafeExecute<T>(Func<DbSet<TEntity>, Task<T>> operations, CancellationToken token = default)
        {
            await using var transaction = await Context.Database.BeginTransactionAsync(token);

            try
            {
                var result = await operations(DbSet);

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

        protected async Task<T> SafeExecute<T>(Func<DbSet<TEntity>, T> operations, CancellationToken token = default)
        {
            await using var transaction = await Context.Database.BeginTransactionAsync(token);

            try
            {
                var result = operations(DbSet);

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
