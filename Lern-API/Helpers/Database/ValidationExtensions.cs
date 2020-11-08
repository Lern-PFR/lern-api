using System;
using FluentValidation;
using Lern_API.Models;
using Lern_API.Services;

namespace Lern_API.Helpers.Database
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, Guid?> MustExist<T, TForeign>(this IRuleBuilder<T, Guid?> builder, IService<TForeign> service) where TForeign : AbstractModel
        {
            return builder.MustAsync(async (x, t) => !x.HasValue || await service.Exists(x.Value, t));
        }
    }
}
