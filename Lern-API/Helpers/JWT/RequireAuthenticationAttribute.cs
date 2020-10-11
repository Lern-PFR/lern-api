using System;
using Lern_API.DataTransferObjects.Responses;
using Lern_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lern_API.Helpers.JWT
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireAuthenticationAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = (User) context.HttpContext.Items["User"];

            if (user == null)
                context.Result = new JsonResult(new ErrorResponse("Unauthorized")) { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}
