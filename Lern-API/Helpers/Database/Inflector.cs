using System;
using System.Linq.Expressions;
using Lern_API.Models;
using PetaPoco.Core.Inflection;

namespace Lern_API.Helpers.Database
{
    public static class Inflector
    {
        private static IInflector Instance { get; } = new EnglishInflector();

        public static string Table(IInflector inflector, string name) => inflector.Pluralise(inflector.Underscore(name));

        public static string Table<T>() where T : AbstractModel
        {
            return Table(Instance, typeof(T).Name);
        }

        public static string Column(IInflector inflector, string name) => inflector.Camelise(name);

        public static string Column(Expression<Func<object>> propExpression)
        {
            string name;

            if (propExpression.Body is MemberExpression expr) {
                name = expr.Member.Name;
            }
            else {
                var op = ((UnaryExpression) propExpression.Body).Operand;
                name = ((MemberExpression) op).Member.Name;
            }

            return Column(Instance, name);
        }

        public static string Column(string name)
        {
            return Column(Instance, name);
        }
    }
}
