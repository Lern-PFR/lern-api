using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services.Database
{
    public interface IAbstractDatabaseService<TEntity> where TEntity : class
    {
        Task<T> ExecuteTransaction<T>(Func<DbSet<TEntity>, Task<T>> operations, CancellationToken token = default);
        Task<bool> ExecuteTransaction(Func<DbSet<TEntity>, Task> operations, CancellationToken token = default);
        Task<T> ExecuteTransaction<T>(Func<DbSet<TEntity>, T> operations, CancellationToken token = default);
        Task<bool> ExecuteTransaction(Action<DbSet<TEntity>> operations, CancellationToken token = default);
        Task<T> ExecuteQuery<T>(Func<IQueryable<TEntity>, Task<T>> query, CancellationToken token = default);
        T ExecuteQuery<T>(Func<IQueryable<TEntity>, T> query);
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

        protected virtual IQueryable<TEntity> WithDefaultIncludes(DbSet<TEntity> set)
        {
            return set;
        }

        public async Task<T> ExecuteTransaction<T>(Func<DbSet<TEntity>, Task<T>> operations, CancellationToken token = default)
        {
            return await SafeExecute(operations, token);
        }

        public async Task<bool> ExecuteTransaction(Func<DbSet<TEntity>, Task> operations, CancellationToken token = default)
        {
            return await SafeExecute(operations, token);
        }

        public async Task<T> ExecuteTransaction<T>(Func<DbSet<TEntity>, T> operations, CancellationToken token = default)
        {
            return await SafeExecute(operations, token);
        }

        public async Task<bool> ExecuteTransaction(Action<DbSet<TEntity>> operations, CancellationToken token = default)
        {
            return await SafeExecute(operations, token);
        }

        public async Task<T> ExecuteQuery<T>(Func<IQueryable<TEntity>, Task<T>> query, CancellationToken token = default)
        {
            try
            {
                return await query(WithDefaultIncludes(DbSet));
            }
#pragma warning disable CS0168 // La variable est déclarée mais jamais utilisée
            catch (Exception _)
#pragma warning restore CS0168 // La variable est déclarée mais jamais utilisée
            {
                return default;
            }
        }

        public T ExecuteQuery<T>(Func<IQueryable<TEntity>, T> query)
        {
            try
            {
                return query(WithDefaultIncludes(DbSet));
            }
#pragma warning disable CS0168 // La variable est déclarée mais jamais utilisée
            catch (Exception _)
#pragma warning restore CS0168 // La variable est déclarée mais jamais utilisée
            {
                return default;
            }
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
#pragma warning disable CS0168 // La variable est déclarée mais jamais utilisée
            catch (Exception _)
#pragma warning restore CS0168 // La variable est déclarée mais jamais utilisée
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
#pragma warning disable CS0168 // La variable est déclarée mais jamais utilisée
            catch (Exception _)
#pragma warning restore CS0168 // La variable est déclarée mais jamais utilisée
            {
                await transaction.RollbackAsync(token);
                return default;
            }
        }

        protected async Task<bool> SafeExecute(Action<DbSet<TEntity>> operations, CancellationToken token = default)
        {
            await using var transaction = await Context.Database.BeginTransactionAsync(token);

            try
            {
                operations(DbSet);

                await Context.SaveChangesAsync(token);
                await transaction.CommitAsync(token);

                return true;
            }
#pragma warning disable CS0168 // La variable est déclarée mais jamais utilisée
            catch (Exception _)
#pragma warning restore CS0168 // La variable est déclarée mais jamais utilisée
            {
                await transaction.RollbackAsync(token);

                return false;
            }
        }

        protected async Task<bool> SafeExecute(Func<DbSet<TEntity>, Task> operations, CancellationToken token = default)
        {
            await using var transaction = await Context.Database.BeginTransactionAsync(token);

            try
            {
                await operations(DbSet);

                await Context.SaveChangesAsync(token);
                await transaction.CommitAsync(token);

                return true;
            }
#pragma warning disable CS0168 // La variable est déclarée mais jamais utilisée
            catch (Exception _)
#pragma warning restore CS0168 // La variable est déclarée mais jamais utilisée
            {
                await transaction.RollbackAsync(token);

                return false;
            }
        }
    }
}
