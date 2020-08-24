using System.Diagnostics.CodeAnalysis;
using Nancy;

namespace Lern_API.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class HttpStatusResponse
    {
        public int StatusCode { get; }

        public HttpStatusResponse(HttpStatusCode status)
        {
            StatusCode = (int) status;
        }
    }
}
