using System;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Validation;

namespace Lern_API.Utilities
{
    public static class ModuleExtensions
    {
        public static object RunHandler<T>(this NancyModule module, Func<T, object> handler)
        {
            T model;

            try
            {
                model = module.BindAndValidate<T>();

                if (!module.ModelValidationResult.IsValid)
                {
                    return module.Negotiate.RespondWithValidationFailure(module.ModelValidationResult);
                }
            }
            catch (ModelBindingException)
            {
                return module.Negotiate.RespondWithValidationFailure("Invalid types received");
            }

            var result = handler(model);

            return RespondWithResult(module, result);
        }

        public static object RunHandler(this NancyModule module, Func<object> handler)
        {
            var result = handler();

            return RespondWithResult(module, result);
        }

        public static async Task<object> RunHandlerAsync<T>(this NancyModule module, Func<T, Task<object>> handler)
        {
            T model;
            
            try
            {
                model = module.BindAndValidate<T>();

                if (!module.ModelValidationResult.IsValid)
                {
                    return module.Negotiate.RespondWithValidationFailure(module.ModelValidationResult);
                }
            }
            catch (ModelBindingException)
            {
                return module.Negotiate.RespondWithValidationFailure("Invalid types received");
            }

            var result = await handler(model);

            return RespondWithResult(module, result);
        }

        public static async Task<object> RunHandlerAsync(this NancyModule module, Func<Task<object>> handler)
        {
            var result = await handler();

            return RespondWithResult(module, result);
        }

        public static Negotiator RespondWithValidationFailure(this Negotiator negotiate, ModelValidationResult validationResult)
        {
            var model = new ValidationFailedResponse(validationResult);

            return negotiate
                .WithModel(model)
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        public static object RespondWithValidationFailure(this Negotiator negotiate, string message)
        {
            var model = new ValidationFailedResponse(message);

            return negotiate
                .WithModel(model)
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        public static object RespondWithResult(this NancyModule module, object result)
        {
            if (result is HttpStatusCode status)
                return module.Negotiate.WithAllowedMediaRange(new MediaRange("application/json")).WithModel(new HttpStatusResponse(status)).WithStatusCode(status);
            
            return module.Response.AsJson(result);
        }

        public static void GetHandler<T>(this NancyModule module, string path, Func<T, object> handler)
        {
            module.Get(path, _ => RunHandler(module, handler));
        }

        public static void GetHandler(this NancyModule module, string path, Func<object> handler)
        {
            module.Get(path, _ => RunHandler(module, handler));
        }

        public static void GetHandlerAsync<T>(this NancyModule module, string path, Func<T, Task<object>> handler)
        {
            module.Get(path, async _ => await RunHandlerAsync(module, handler));
        }

        public static void GetHandlerAsync(this NancyModule module, string path, Func<Task<object>> handler)
        {
            module.Get(path, async _ => await RunHandlerAsync(module, handler));
        }

        public static void PostHandler<T>(this NancyModule module, string path, Func<T, object> handler)
        {
            module.Post(path, _ => RunHandler(module, handler));
        }

        public static void PostHandler(this NancyModule module, string path, Func<object> handler)
        {
            module.Post(path, _ => RunHandler(module, handler));
        }

        public static void PostHandlerAsync<T>(this NancyModule module, string path, Func<T, Task<object>> handler)
        {
            module.Post(path, async _ => await RunHandlerAsync(module, handler));
        }

        public static void PostHandlerAsync(this NancyModule module, string path, Func<Task<object>> handler)
        {
            module.Post(path, async _ => await RunHandlerAsync(module, handler));
        }

        public static void PutHandler<T>(this NancyModule module, string path, Func<T, object> handler)
        {
            module.Put(path, _ => RunHandler(module, handler));
        }

        public static void PutHandler(this NancyModule module, string path, Func<object> handler)
        {
            module.Put(path, _ => RunHandler(module, handler));
        }

        public static void PutHandlerAsync<T>(this NancyModule module, string path, Func<T, Task<object>> handler)
        {
            module.Put(path, async _ => await RunHandlerAsync(module, handler));
        }

        public static void PutHandlerAsync(this NancyModule module, string path, Func<Task<object>> handler)
        {
            module.Put(path, async _ => await RunHandlerAsync(module, handler));
        }

        public static void PatchHandler<T>(this NancyModule module, string path, Func<T, object> handler)
        {
            module.Patch(path, _ => RunHandler(module, handler));
        }

        public static void PatchHandler(this NancyModule module, string path, Func<object> handler)
        {
            module.Patch(path, _ => RunHandler(module, handler));
        }

        public static void PatchHandlerAsync<T>(this NancyModule module, string path, Func<T, Task<object>> handler)
        {
            module.Patch(path, async _ => await RunHandlerAsync(module, handler));
        }

        public static void PatchHandlerAsync(this NancyModule module, string path, Func<Task<object>> handler)
        {
            module.Patch(path, async _ => await RunHandlerAsync(module, handler));
        }

        public static void DeleteHandler<T>(this NancyModule module, string path, Func<T, object> handler)
        {
            module.Delete(path, _ => RunHandler(module, handler));
        }

        public static void DeleteHandler(this NancyModule module, string path, Func<object> handler)
        {
            module.Delete(path, _ => RunHandler(module, handler));
        }

        public static void DeleteHandlerAsync<T>(this NancyModule module, string path, Func<T, Task<object>> handler)
        {
            module.Delete(path, async _ => await RunHandlerAsync(module, handler));
        }

        public static void DeleteHandlerAsync(this NancyModule module, string path, Func<Task<object>> handler)
        {
            module.Delete(path, async _ => await RunHandlerAsync(module, handler));
        }
    }
}
