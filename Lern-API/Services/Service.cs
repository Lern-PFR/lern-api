using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.Models;
using Lern_API.Repositories;
using Microsoft.Extensions.Logging;

namespace Lern_API.Services
{
    public interface IService<TEntity, TRepository> : IService<TEntity> where TEntity : AbstractModel
    {

    }

    public interface IService<TEntity> where TEntity : AbstractModel
    {
        Task<IEnumerable<TEntity>> GetAll(CancellationToken token = default);
        Task<TEntity> Get(Guid id, CancellationToken token = default);
        Task<Guid> Create(TEntity entity, CancellationToken token = default, bool ignoreReadOnly = false);
        Task<TEntity> Update(TEntity entity, IEnumerable<string> columns, CancellationToken token = default);
        Task<bool> Delete(Guid id, CancellationToken token = default);
        Task<bool> Exists(Guid id, CancellationToken token = default);
    }

    public class Service<TEntity> : Service<TEntity, IRepository<TEntity>> where TEntity : AbstractModel
    {
        public Service(ILogger<Service<TEntity, IRepository<TEntity>>> logger, IRepository<TEntity> repository) : base(logger, repository)
        {

        }
    }

    public class Service<TEntity, TRepository> : IService<TEntity> where TEntity : AbstractModel where TRepository : IRepository<TEntity>
    {
        protected ILogger Log { get; }
        protected TRepository Repository { get; }

        public Service(ILogger<Service<TEntity, TRepository>> logger, TRepository repository)
        {
            Log = logger;
            Repository = repository;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAll(CancellationToken token = default)
        {
            return await Repository.All(token);
        }

        public virtual async Task<TEntity> Get(Guid id, CancellationToken token = default)
        {
            return await Repository.Get(id, token);
        }

        public virtual async Task<Guid> Create(TEntity entity, CancellationToken token = default, bool ignoreReadOnly = false)
        {
            return await Repository.Create(entity, token, ignoreReadOnly);
        }

        public virtual async Task<TEntity> Update(TEntity entity, IEnumerable<string> columns, CancellationToken token = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            columns = columns.Concat(new []{ nameof(AbstractModel.UpdatedAt) });

            return await Repository.Update(entity, columns, token);
        }

        public virtual async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            return await Repository.Delete(id, token);
        }

        public virtual async Task<bool> Exists(Guid id, CancellationToken token = default)
        {
            return await Repository.Exists(id, token);
        }
    }
}
