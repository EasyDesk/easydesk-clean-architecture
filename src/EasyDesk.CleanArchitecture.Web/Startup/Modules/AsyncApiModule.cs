using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Web.Neuroglia;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.AsyncApi;
using Neuroglia.AsyncApi.Configuration;
using Neuroglia.AsyncApi.Services.FluentBuilders;
using Rebus.Topic;

namespace EasyDesk.CleanArchitecture.Web.Startup.Modules;
public class AsyncApiModule : AppModule
{
    private readonly Action<IAsyncApiGenerationOptionsBuilder> _configure;

    public AsyncApiModule(Action<IAsyncApiGenerationOptionsBuilder> configure = null)
    {
        _configure = configure;
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddSingleton(p => new AsyncApiDocumentGenerator(
            p.GetRequiredService<IAsyncApiDocumentBuilder>(),
            p.GetRequiredService<KnownMessageTypes>(),
            app.Name,
            p.GetRequiredService<ITopicNameConvention>()));
        services.AddAsyncApiGeneration(options =>
        {
            SetupAsyncApiDocs(app, options);
            _configure?.Invoke(options);
        });
        services.AddAsyncApiUI();
    }

    private void SetupAsyncApiDocs(AppDescription app, IAsyncApiGenerationOptionsBuilder options)
    {
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
    }
}
