using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lern_API.Models;
using Lern_API.Repositories;
using Microsoft.Extensions.Logging;

namespace Lern_API.Services
{
    public interface IService<TEntity, TRepository> where TEntity : AbstractModel where TRepository : IRepository<TEntity>
    {
        Task<IEnumerable<TEntity>> GetAll();
        Task<TEntity> Get(Guid id);
        Task<Guid> Create(TEntity entity);
        Task<TEntity> Update(TEntity entity, IEnumerable<string> columns);
        Task<bool> Delete(TEntity entity);
    }

    public class Service<TEntity, TRepository> : IService<TEntity, TRepository> where TEntity : AbstractModel where TRepository : IRepository<TEntity>
    {
        protected ILogger Log { get; }
        protected TRepository Repository { get; }

        public Service(ILogger<Service<TEntity, TRepository>> logger, TRepository repository)
        {
            Log = logger;
            Repository = repository;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAll()
        {
            return await Repository.All();
        }

        public virtual async Task<TEntity> Get(Guid id)
        {
            return await Repository.Get(id);
        }

        public virtual async Task<Guid> Create(TEntity entity)
        {
            return await Repository.Create(entity);
        }

        public virtual async Task<TEntity> Update(TEntity entity, IEnumerable<string> columns)
        {
            return await Repository.Update(entity, columns);
        }

        public virtual async Task<bool> Delete(TEntity entity)
        {
            return await Repository.Delete(entity);
        }
    }
}
