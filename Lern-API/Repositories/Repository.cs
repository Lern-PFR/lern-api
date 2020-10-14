using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Lern_API.Helpers.Swagger;
using Lern_API.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using PetaPoco;
using PetaPoco.Core.Inflection;

namespace Lern_API.Repositories
{
    public interface IRepository<TEntity> where TEntity : AbstractModel
    {
        Task<IEnumerable<TEntity>> All();
        Task<TEntity> Get(Guid id);
        Task<Guid> Create(TEntity entity);
        Task<TEntity> Update(TEntity entity, IEnumerable<string> columns);
        Task<bool> Delete(Guid id);
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
            if (entity.Id == default)
                entity.Id = Guid.NewGuid();

            var inflector = new EnglishInflector();
            var table = inflector.Pluralise(inflector.Underscore(typeof(TEntity).Name));
            var primaryKey = inflector.Camelise(nameof(entity.Id));

            dynamic poco = new ExpandoObject();
            var pocoDict = (IDictionary<string, object>) poco;

            foreach (var prop in typeof(TEntity).GetProperties())
            {
                if (prop.GetCustomAttributes(typeof(ReadOnlyAttribute), true).SingleOrDefault() != null)
                    continue;

                pocoDict.Add(inflector.Camelise(prop.Name), prop.GetValue(entity));
            }

            pocoDict.Add(primaryKey, entity.Id);

            var result = await RunOrDefault(async () => await Database.InsertAsync(table, primaryKey, false, poco));

            return result switch
            {
                null => Guid.Empty,
                Guid id => id,
                _ => Guid.TryParse(result.ToString(), out Guid guid) ? guid : Guid.Empty
            };
        }

        public async Task<TEntity> Update(TEntity entity, IEnumerable<string> columns)
        {
            var inflector = new EnglishInflector();
            var readOnlyProperties = typeof(TEntity).GetProperties().Where(x =>
                x.GetCustomAttributes(typeof(ReadOnlyAttribute), true).SingleOrDefault() != null).Select(x => inflector.Camelise(x.Name));
            columns = columns.Where(x => !readOnlyProperties.Contains(x));

            var done = await RunOrDefault(async () => await Database.UpdateAsync(entity, columns)) == 1;

            if (done)
                return await RunOrDefault(async () => await Database.SingleOrDefaultAsync<TEntity>(entity.Id));
            
            return default;
        }

        public async Task<bool> Delete(Guid id)
        {
            return await RunOrDefault(async () => await Database.DeleteAsync<TEntity>(id)) == 1;
        }
    }
}
