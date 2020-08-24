using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lern_API.Models;
using Lern_API.Utilities;
using Npgsql;
using NullGuard;
using PetaPoco;

namespace Lern_API.Repositories
{
    public interface IRepository<T>
    {
        IEnumerable<T> All();
        Task<IEnumerable<T>> AllAsync();
        User Get(int id);
        Task<T> GetAsync(int id);
        int Create(T user);
        Task<int> CreateAsync(T user);
        bool Update(T user);
        Task<bool> UpdateAsync(T user);
        bool Delete(T user);
        Task<bool> DeleteAsync(T user);
    }

    public abstract class Repository
    {
        private ILogger Log { get; }
        protected IDatabase Database { get; }

        protected Repository(ILogger logger, IDatabase database)
        {
            Log = logger;
            Database = database;
        }

        [return: AllowNull]
        public T RunOrDefault<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception e) when (e is AggregateException || e is PostgresException)
            {
                Log.Warning($"Erreur de base de données : {e.Message}");
                return default;
            }
        }

        [return: AllowNull]
        public async Task<T> RunOrDefault<T>(Func<Task<T>> func)
        {
            try
            {
                return await func();
            }
            catch (Exception e) when (e is AggregateException || e is PostgresException)
            {
                Log.Warning($"Erreur de base de données : {e.Message}");
                return default;
            }
        }
    }
}
