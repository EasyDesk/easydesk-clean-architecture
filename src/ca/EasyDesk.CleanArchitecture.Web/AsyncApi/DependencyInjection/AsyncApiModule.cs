using Autofac;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;
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

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterType<KnownTypesDocumentGenerator>()
            .As<IDocumentGenerator>()
            .InstancePerDependency();

        builder.RegisterType<KnownTypesDocumentProvider>()
            .As<IAsyncApiDocumentProvider>()
            .InstancePerDependency();
    }

    protected override void ConfigureServices(AppDescription app, IServiceCollection services)
    {
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
    public static IAppBuilder AddAsyncApi(this IAppBuilder builder, Action<AsyncApiOptions>? configure = null)
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
