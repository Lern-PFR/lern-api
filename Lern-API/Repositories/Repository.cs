using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lern_API.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using PetaPoco;

namespace Lern_API.Repositories
{
    public interface IRepository<TEntity> where TEntity : AbstractModel
    {
        Task<IEnumerable<TEntity>> All();
        Task<TEntity> Get(Guid id);
        Task<Guid> Create(TEntity entity);
        Task<bool> Update(TEntity entity);
        Task<bool> Delete(TEntity entity);
    }

    public class Repository<TEntity> : IRepository<TEntity> where TEntity : AbstractModel
    {
        private ILogger Log { get; }
        protected IDatabase Database { get; }

        public Repository(ILogger<Repository<TEntity>> logger, IDatabase database)
        {
            Log = logger;
            Database = database;
        }

        public async Task<T> RunOrDefault<T>(Func<Task<T>> func)
        {
            try
            {
                return await func();
            }
            catch (Exception e) when (e is AggregateException || e is PostgresException)
            {
                Log.LogError($"Erreur de base de données : {e.Message}");
                return default;
            }
        }

        public async Task<IEnumerable<TEntity>> All()
        {
            return await RunOrDefault(async () => await Database.FetchAsync<TEntity>());
        }

        public async Task<TEntity> Get(Guid id)
        {
            return await RunOrDefault(async () => await Database.SingleOrDefaultAsync<TEntity>(id));
        }

        public async Task<Guid> Create(TEntity entity)
        {
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            var result = await RunOrDefault(async () => await Database.InsertAsync(entity));

            return result switch
            {
                null => Guid.Empty,
                Guid id => id,
                _ => Guid.TryParse(result.ToString(), out var guid) ? guid : Guid.Empty
            };
        }

        public async Task<bool> Update(TEntity entity)
        {
            return await RunOrDefault(async () => await Database.UpdateAsync(entity)) == 1;
        }

        public async Task<bool> Delete(TEntity entity)
        {
            return await RunOrDefault(async () => await Database.DeleteAsync<TEntity>(entity)) == 1;
        }
    }
}
