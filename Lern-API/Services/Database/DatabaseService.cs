﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.Helpers.JWT;
using Lern_API.Helpers.Models;
using Lern_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services.Database
{
    public interface IDatabaseService<TEntity, in TDataTransferObject> : IAbstractDatabaseService<TEntity> where TEntity : class, IModelBase, new()
    {
        Task<IEnumerable<TEntity>> GetAll(CancellationToken token = default);
        Task<TEntity> Get(Guid id, CancellationToken token = default);
        Task<TEntity> Create(TDataTransferObject entity, CancellationToken token = default);
        Task<TEntity> Update(Guid id, TDataTransferObject entity, CancellationToken token = default);
        Task<bool> Delete(Guid id, CancellationToken token = default);
        Task<bool> Exists(Guid id, CancellationToken token = default);
    }

    public class DatabaseService<TEntity, TDataTransferObject> : AbstractDatabaseService<TEntity>, IDatabaseService<TEntity, TDataTransferObject> where TEntity : class, IModelBase, new()
    {
        protected IHttpContextAccessor HttpContextAccessor { get; }

        public DatabaseService(LernContext context, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        public virtual async Task<TEntity> Create(TDataTransferObject entity, CancellationToken token = default)
        {
            var final = new TEntity();
            final.CloneFrom(entity);

            var authorId = final.GetType().GetProperty("AuthorId");

            if (authorId != null && authorId.PropertyType == typeof(Guid))
            {
                authorId.SetValue(final, HttpContextAccessor.HttpContext.GetUser().Id);
            }

            var entityEntry = await SafeExecute(async set => await set.AddAsync(final, token), token);

            return entityEntry?.Entity;
        }

        public virtual async Task<bool> Delete(Guid id, CancellationToken token = default)
        {
            var exists = await Exists(id, token);

            if (!exists)
                return false;

            var result = await SafeExecute(async set => set.Remove(await Get(id, token)), token);

            return !result.Equals(default);
        }

        public virtual async Task<bool> Exists(Guid id, CancellationToken token = default)
        {
            return await DbSet.AnyAsync(x => x.Id == id, token);
        }

        public virtual async Task<TEntity> Get(Guid id, CancellationToken token = default)
        {
            return await WithDefaultIncludes(DbSet).FirstOrDefaultAsync(x => x.Id == id, token);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAll(CancellationToken token = default)
        {
            return await WithDefaultIncludes(DbSet).ToListAsync(token);
        }

        public virtual async Task<TEntity> Update(Guid id, TDataTransferObject entity, CancellationToken token = default)
        {
            var entry = await Get(id, token);
            
            entry.CloneFrom(entity);

            var result = await SafeExecute(set => set.Update(entry), token);

            return result?.Entity;
        }
    }
}
