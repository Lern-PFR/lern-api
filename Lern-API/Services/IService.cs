using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lern_API.Services
{
    public interface IService<TEntity>
    {
        Task<IEnumerable<TEntity>> GetAll(CancellationToken token = default);
        Task<TEntity> Get(Guid id, CancellationToken token = default);
        Task<TEntity> Create(TEntity entity, CancellationToken token = default);
        Task<TEntity> Update(TEntity entity, IEnumerable<string> columns, CancellationToken token = default);
        Task<bool> Delete(Guid id, CancellationToken token = default);
        Task<bool> Exists(Guid id, CancellationToken token = default);
    }
}
