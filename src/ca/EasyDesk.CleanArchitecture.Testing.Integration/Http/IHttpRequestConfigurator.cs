using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public interface IHttpRequestConfigurator
{
    void ConfigureHttpRequest(HttpRequestBuilder request);
}
