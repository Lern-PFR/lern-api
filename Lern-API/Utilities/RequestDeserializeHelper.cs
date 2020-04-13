using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;

namespace Lern_API.Utilities
{
    public static class RequestDeserializeHelper
    {
        public static T DeserializeTo<T>(this Request request, T anonymousType)
        {
            return JsonConvert.DeserializeAnonymousType(request.Body.AsString(), anonymousType);
        }

        public static T DeserializeTo<T>(this Request request)
        {
            return JsonConvert.DeserializeObject<T>(request.Body.AsString());
        }
    }
}
