using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.Helpers.Database;
using Lern_API.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using PetaPoco;

namespace Lern_API.Repositories
{
    public interface IRepository<TEntity> where TEntity : AbstractModel
    {
        Task<IEnumerable<TEntity>> All(CancellationToken token = default);
        Task<TEntity> Get(Guid id, CancellationToken token = default);
        Task<Guid> Create(TEntity entity, CancellationToken token = default, bool ignoreReadOnly = false);
        Task<TEntity> Update(TEntity entity, IEnumerable<string> columns, CancellationToken token = default);
        Task<bool> Delete(Guid id, CancellationToken token = default);
        Task<bool> Exists(Guid id, CancellationToken token = default);
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

        public async Task<IEnumerable<TEntity>> All(CancellationToken token = default)
        {
            return await RunOrDefault(async () => await Database.FetchAsync<TEntity>(token));
        }

        public async Task<TEntity> Get(Guid id, CancellationToken token = default)
        {
            return await RunOrDefault(async () => await Database.SingleOrDefaultAsync<TEntity>(token, id));
        }

        public async Task<Guid> Create(TEntity entity, CancellationToken token = default, bool ignoreReadOnly = false)
        {
            if (entity.Id == default)
                entity.Id = Guid.NewGuid();

            object result;

            if (!ignoreReadOnly)
            {
                var table = Inflector.Table<TEntity>();
                var primaryKey = Inflector.Column(nameof(entity.Id));

                dynamic poco = new ExpandoObject();
                var pocoDict = (IDictionary<string, object>) poco;

                foreach (var prop in typeof(TEntity).GetProperties())
                {
                    if (prop.GetCustomAttributes(typeof(ReadOnlyAttribute), true).SingleOrDefault() != null)
                        continue;

                    pocoDict.Add(Inflector.Column(prop.Name), prop.GetValue(entity));
                }

                pocoDict.Add(primaryKey, entity.Id);

                result = await RunOrDefault(async () => await Database.InsertAsync(token, table, primaryKey, false, poco));
            }
            else
            {
                result = await RunOrDefault(async () => await Database.InsertAsync(token, entity));
            }

            return result switch
            {
                null => Guid.Empty,
                Guid id => id,
                _ => Guid.TryParse(result.ToString(), out Guid guid) ? guid : Guid.Empty
            };
        }

        public async Task<TEntity> Update(TEntity entity, IEnumerable<string> columns, CancellationToken token = default)
        {
            var readOnlyProperties = typeof(TEntity).GetProperties().Where(x =>
                x.GetCustomAttributes(typeof(ReadOnlyAttribute), true).SingleOrDefault() != null).Select(x => Inflector.Column(x.Name));
            columns = columns.Where(x => !readOnlyProperties.Contains(x));

            var done = await RunOrDefault(async () => await Database.UpdateAsync(token, entity, columns)) == 1;

            if (done)
                return await RunOrDefault(async () => await Database.SingleOrDefaultAsync<TEntity>(token, entity.Id));
            
            return default;
        }

        public async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            return await RunOrDefault(async () => await Database.DeleteAsync<TEntity>(token, id)) == 1;
        }

        public async Task<bool> Exists(Guid id, CancellationToken token = default)
        {
            return await RunOrDefault(async () => await Database.ExistsAsync<TEntity>(token, id));
        }
    }
}
