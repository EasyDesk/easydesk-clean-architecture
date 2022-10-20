using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Saunter;
using Saunter.Generation;

namespace EasyDesk.CleanArchitecture.Web.AsyncApi.DependencyInjection;

public class AsyncApiModule : AppModule
{
    private readonly Action<AsyncApiOptions> _configure;

    public AsyncApiModule(Action<AsyncApiOptions> configure = null)
    {
        _configure = configure;
    }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.RequireModule<RebusMessagingModule>();
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddTransient<IDocumentGenerator>(p => new KnownTypesDocumentGenerator(
            app.Name,
            p.GetRequiredService<RebusMessagingOptions>().InputQueueAddress));
        services.AddTransient<IAsyncApiDocumentProvider, KnowTypesDocumentProvider>();
        services.AddAsyncApiSchemaGeneration(options =>
        {
            options.Middleware.Route = "/asyncapi/asyncapi.json";
            options.Middleware.UiBaseRoute = "/asyncapi/";
            options.Middleware.UiTitle = $"AsyncApi :: {app.Name}";
            _configure?.Invoke(options);
        });
    }
}

public static class AsyncApiModuleExtensions
{
    public static AppBuilder AddAsyncApi(this AppBuilder builder, Action<AsyncApiOptions> configure = null)
    {
        return builder.AddModule(new AsyncApiModule(configure));
    }

    public static bool HasAsyncApi(this AppDescription app) => app.HasModule<AsyncApiModule>();

    public static void UseAsyncApiModule(this WebApplication app)
    {
        app.MapAsyncApiDocuments();
        app.MapAsyncApiUi();
    }
}
