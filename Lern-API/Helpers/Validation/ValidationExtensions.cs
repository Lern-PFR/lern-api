using System;
using FluentValidation;
using Lern_API.Models;
using Lern_API.Services;

namespace Lern_API.Helpers.Validation
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, Guid> MustExistInDatabase<T, TEntity, TEntityRequest>(this IRuleBuilder<T, Guid> rule, IService<TEntity, TEntityRequest> service)
            where TEntity : class, IModelBase, new()
        {
            return rule.MustAsync(async (_, id, token) => await service.Exists(id, token)).WithMessage($"Provided {typeof(T).Name} does not exist");
        }

        public static IRuleBuilderOptions<T, Guid?> MustExistInDatabaseIfNotNull<T, TEntity, TEntityRequest>(this IRuleBuilder<T, Guid?> rule, IService<TEntity, TEntityRequest> service)
            where TEntity : class, IModelBase, new()
        {
            return rule.MustAsync(async (_, id, token) =>
            {
                if (id != null)
                    return await service.Exists(id.Value, token);
                
                return true;
            }).WithMessage($"Provided {typeof(T).Name} does not exist");
        }
    }
}
