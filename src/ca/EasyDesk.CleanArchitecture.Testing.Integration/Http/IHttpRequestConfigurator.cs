using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public interface IHttpRequestConfigurator
{
    void ConfigureHttpRequest(HttpRequestBuilder request);
}

internal class HttpRequestConfigurator : IHttpRequestConfigurator
{
    private readonly Action<HttpRequestBuilder> _configure;

    public HttpRequestConfigurator(Action<HttpRequestBuilder> configure)
    {
        _configure = configure;
    }

    public void ConfigureHttpRequest(HttpRequestBuilder request) =>
        _configure(request);
}
