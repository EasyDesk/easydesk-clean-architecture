using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Web.AsyncApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.AsyncApi;
using Neuroglia.AsyncApi.Configuration;
using Neuroglia.AsyncApi.Services.FluentBuilders;
using Neuroglia.AsyncApi.Services.Generators;

namespace EasyDesk.CleanArchitecture.Web.Modules;
public class AsyncApiModule : AppModule
{
    private readonly Action<IAsyncApiGenerationOptionsBuilder> _configure;

    public AsyncApiModule(Action<IAsyncApiGenerationOptionsBuilder> configure = null)
    {
        _configure = configure;
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddAsyncApiGeneration(options => _configure?.Invoke(options));
        services.AddAsyncApiUI();
        services.AddTransient<IAsyncApiDocumentGenerator>(p => new KnownTypesDocumentGenerator(
            p.GetRequiredService<IAsyncApiDocumentBuilder>(),
            p.GetRequiredService<KnownMessageTypes>(),
            app.Name,
            p.GetRequiredService<RebusMessagingOptions>().InputQueueAddress));
    }
}

public static class AsyncApiModuleExtensions
{
    public static AppBuilder AddAsyncApi(this AppBuilder builder, Action<IAsyncApiGenerationOptionsBuilder> configure = null)
    {
        return builder.AddModule(new AsyncApiModule(configure));
    }

    public static bool HasAsyncApi(this AppDescription app) => app.HasModule<AsyncApiModule>();

    public static void UseAsyncApiModule(this WebApplication app)
    {
        app.UseAsyncApiGeneration();
        app.MapRazorPages();
    }
}
