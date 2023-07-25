using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using Saunter.Generation;

namespace EasyDesk.CleanArchitecture.Web.AsyncApi.DependencyInjection;

public class AsyncApiModule : AppModule
{
    private readonly Action<AsyncApiOptions>? _configure;

    public AsyncApiModule(Action<AsyncApiOptions>? configure = null)
    {
        _configure = configure;
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddTransient<IDocumentGenerator, KnownTypesDocumentGenerator>();
        services.AddTransient<IAsyncApiDocumentProvider, KnownTypesDocumentProvider>();
        services.AddAsyncApiSchemaGeneration(options =>
        {
            options.Middleware.Route = "/asyncapi/{document}/asyncapi.json";
            options.Middleware.UiBaseRoute = "/asyncapi/{document}/";
            options.Middleware.UiTitle = $"{app.Name} - AsyncAPI";
            _configure?.Invoke(options);
        });
        app.RequireModule<RebusMessagingModule>().Options.KnownMessageTypes
            .GetSupportedApiVersionsFromNamespaces()
            .ForEach(v =>
            {
                services.ConfigureNamedAsyncApi(v.ToString(), doc =>
                {
                    doc.Info = new Info(app.Name, v.ToString());
                });
            });
    }
}

public static class AsyncApiModuleExtensions
{
    public static AppBuilder AddAsyncApi(this AppBuilder builder, Action<AsyncApiOptions>? configure = null)
    {
        return builder.AddModule(new AsyncApiModule(configure));
    }

    public static bool HasAsyncApi(this AppDescription app) => app.HasModule<AsyncApiModule>();

    public static void UseAsyncApiModule(this WebApplication app)
    {
        app.MapAsyncApiDocuments();
        app.MapAsyncApiUi();
        app.Services.GetRequiredService<RebusMessagingOptions>().KnownMessageTypes
            .GetSupportedApiVersionsFromNamespaces()
            .MaxOption()
            .IfPresent(v =>
            {
                Task Action(HttpContext context)
                {
                    context.Response.Redirect($"/asyncapi/{v}", permanent: true);
                    return Task.CompletedTask;
                }
                app.MapGet("/asyncapi", Action);
                app.MapGet("/asyncapi/index.html", Action);
            });
    }
}
