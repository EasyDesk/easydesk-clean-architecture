using Autofac;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Host;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Testing.Integration.OpenApi;

public static class OpenApiHelperFixtureExtensions
{
    public static TestFixtureConfigurer AddOpenApiTestHelper(this TestFixtureConfigurer configurer)
    {
        configurer.ContainerBuilder
            .Register(c =>
            {
                var openApiEndpoints = c.Resolve<ITestHost>()
                    .LifetimeScope
                    .Resolve<IServiceProvider>()
                    .GetOpenApiEndpoints();

                return new OpenApiTestHelper(c.Resolve<HttpTestHelper>(), openApiEndpoints.ToFixedMap());
            })
            .InstancePerLifetimeScope();

        return configurer;
    }
}

public class OpenApiTestHelper
{
    private readonly HttpTestHelper _http;
    private readonly IFixedMap<string, string> _endpoints;

    public OpenApiTestHelper(HttpTestHelper http, IFixedMap<string, string> endpoints)
    {
        _http = http;
        _endpoints = endpoints;
    }

    public async Task<string> GetOpenApiDocument(string documentKey)
    {
        var response = await _http
            .Get<object>(_endpoints[documentKey])
            .Send()
            .GetResponse();
        return response.Content.AsString();
    }

    public async Task<string> GetSwaggerPage()
    {
        var response = await _http
            .Get<object>(OpenApiModuleExtensions.DefaultRoutePrefix)
            .Send()
            .GetResponse();
        return response.Content.AsString();
    }

    public IEnumerable<string> DocumentKeys => _endpoints.Keys;
}
