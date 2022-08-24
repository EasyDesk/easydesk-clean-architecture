using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Json;
using EasyDesk.CleanArchitecture.Web.Swagger;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools.Collections;
using MicroElements.Swashbuckle.NodaTime;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Startup.Modules;

public class SwaggerModule : AppModule
{
    private readonly Action<SwaggerGenOptions> _configure;

    public SwaggerModule(Action<SwaggerGenOptions> configure = null)
    {
        _configure = configure;
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddSwaggerGenNewtonsoftSupport();
        services.AddSwaggerGen(options =>
        {
            SetupSwaggerDocs(app, options);
            AddNodaTimeSupport(app, options);
            AddAuthenticationSchemesSupport(app, options);

            _configure?.Invoke(options);
        });
    }

    private void SetupSwaggerDocs(AppDescription app, SwaggerGenOptions options)
    {
        if (app.HasModule<ApiVersioningModule>())
        {
            SetupApiVersionedDocs(app, options);
        }
        else
        {
            options.SwaggerDoc("main", new OpenApiInfo
            {
                Title = app.Name
            });
        }
    }

    private void SetupApiVersionedDocs(AppDescription app, SwaggerGenOptions options)
    {
        VersioningUtils.GetSupportedVersions(app)
            .Select(version => version.ToDisplayString())
            .ForEach(version => options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = app.Name,
                Version = version
            }));

        options.OperationFilter<AddApiVersionParameterFilter>();
        options.DocInclusionPredicate((version, api) =>
        {
            if (api.ActionDescriptor.GetApiVersionModel().IsApiVersionNeutral)
            {
                return true;
            }
            if (api.ActionDescriptor is not ControllerActionDescriptor descriptor)
            {
                return false;
            }
            return descriptor
                .ControllerTypeInfo
                .GetControllerVersion()
                .Map(v => v.ToDisplayString())
                .Contains(version);
        });
    }

    private static void AddNodaTimeSupport(AppDescription app, SwaggerGenOptions options)
    {
        var dateTimeZoneProvider = app.GetModule<TimeManagementModule>()
            .Map(m => m.DateTimeZoneProvider)
            .OrElseNull();

        options.ConfigureForNodaTime(
            serializerSettings: JsonDefaults.DefaultSerializerSettings(),
            dateTimeZoneProvider: dateTimeZoneProvider);
    }

    private void AddAuthenticationSchemesSupport(AppDescription app, SwaggerGenOptions options)
    {
        app.GetModule<AuthenticationModule>().IfPresent(auth =>
        {
            auth.Schemes.ForEach(scheme => scheme.Value.ConfigureSwagger(options));
        });
    }
}

public static class SwaggerModuleExtensions
{
    public static AppBuilder AddSwagger(this AppBuilder builder, Action<SwaggerGenOptions> configure = null)
    {
        return builder.AddModule(new SwaggerModule(configure));
    }

    public static bool HasSwagger(this AppDescription app) => app.HasModule<SwaggerModule>();
}
