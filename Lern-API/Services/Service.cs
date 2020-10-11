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
        Task<bool> Update(TEntity entity);
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

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            return await Repository.All();
        }

        public async Task<TEntity> Get(Guid id)
        {
            return await Repository.Get(id);
        }

        public async Task<Guid> Create(TEntity entity)
        {
            return await Repository.Create(entity);
        }

        public async Task<bool> Update(TEntity entity)
        {
            return await Repository.Update(entity);
        }

        public async Task<bool> Delete(TEntity entity)
        {
            return await Repository.Delete(entity);
        }
    }
}
