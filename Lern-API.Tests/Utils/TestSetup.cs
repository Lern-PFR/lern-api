using System;
using FluentAssertions.Equivalency;
using Lern_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Lern_API.Tests.Utils
{
    public static class TestSetup
    {
        public static T SetupController<T>(params object[] parameters) where T : ControllerBase
        {
            var controller = (T) Activator.CreateInstance(typeof(T), parameters);

            if (controller == null)
                throw new Exception("Cannot instantiate this controller with these parameters");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        public static LernContext SetupContext()
        {
            var options = new DbContextOptionsBuilder<LernContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new LernContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        public static IHttpContextAccessor SetupHttpContext()
        {
            var accessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            return accessor;
        }

        public static T SetupSession<T>(this T controller, User user) where T : ControllerBase
        {
            controller.ControllerContext.HttpContext.Items.Add("User", user);
            return controller;
        }

        public static IHttpContextAccessor SetupSession(this IHttpContextAccessor accessor, User user)
        {
            accessor.HttpContext?.Items.Add("User", user);
            return accessor;
        }

        public static Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> IgnoreTimestamps<T>() where T : ITimestamp
        {
            return config => config.Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt);
        }
    }
}
