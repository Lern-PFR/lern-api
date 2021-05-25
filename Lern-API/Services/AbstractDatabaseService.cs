using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services
{
    public interface IAbstractDatabaseService<TEntity> where TEntity : class
    {
        Task<T> ExecuteTransaction<T>(Func<DbSet<TEntity>, Task<T>> operations, CancellationToken token = default);
        Task<T> ExecuteTransaction<T>(Func<DbSet<TEntity>, T> operations, CancellationToken token = default);
    }

    public abstract class AbstractDatabaseService<TEntity> : IAbstractDatabaseService<TEntity> where TEntity : class
    {
        protected LernContext Context { get; }
        protected DbSet<TEntity> DbSet { get; }

        protected AbstractDatabaseService(LernContext context)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
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
            catch (Exception _)
            {
                await transaction.RollbackAsync(token);
                return default;
            }
        }
    }
}
