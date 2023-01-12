using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;

public class HttpSingleResponseWrapper<T> : HttpResponseWrapper<T, Nothing>
{
    public HttpSingleResponseWrapper(AsyncFunc<ImmutableHttpResponseMessage> httpResponseMessage, JsonSerializerSettings jsonSerializerSettings)
        : base(httpResponseMessage, jsonSerializerSettings)
    {
    }
}
