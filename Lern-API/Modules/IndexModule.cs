using Lern_API.DataTransferObjects;
using Nancy;
using Nancy.ModelBinding;

namespace Lern_API.Modules
{
    public sealed class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get("", _ => GetIndex());
            Get("/{path*}", _ =>
            {
                this.BindAndValidate<IndexRequest>();

                return !ModelValidationResult.IsValid ? HttpStatusCode.NotAcceptable : GetIndex();
            });
        }

        public object GetIndex()
        {
            return Negotiate.WithView("Content/index.html");
        }
    }
}
