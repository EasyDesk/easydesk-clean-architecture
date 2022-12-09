using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.Tools.Collections;
using MicroElements.Swashbuckle.NodaTime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;

public class OpenApiModule : AppModule
{
    private readonly Action<SwaggerGenOptions> _configure;

    public OpenApiModule(Action<SwaggerGenOptions> configure = null)
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
        options.SchemaFilter<OptionSchemaFilter>();
        app.GetModule<ApiVersioningModule>().Match(
            some: m => SetupApiVersionedDocs(m, app, options),
            none: () => SetupSingleVersionDoc(app, options));
    }

    private void SetupApiVersionedDocs(ApiVersioningModule module, AppDescription app, SwaggerGenOptions options)
    {
        module.ApiVersioningInfo
            .SupportedVersions
            .Select(version => version.ToDisplayString())
            .ForEach(version => options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = $"{app.Name} {version}",
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

    private void SetupSingleVersionDoc(AppDescription app, SwaggerGenOptions options)
    {
        options.SwaggerDoc("main", new OpenApiInfo
        {
            Title = app.Name
        });
    }

    private void AddNodaTimeSupport(AppDescription app, SwaggerGenOptions options)
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
            auth.Schemes.ForEach(scheme => scheme.Value.ConfigureOpenApi(options));
        });
    }
}

public static class SwaggerModuleExtensions
{
    public static AppBuilder AddOpenApi(this AppBuilder builder, Action<SwaggerGenOptions> configure = null)
    {
        return builder.AddModule(new OpenApiModule(configure));
    }

    public static bool HasOpenApi(this AppDescription app) => app.HasModule<OpenApiModule>();

    public static void UseOpenApiModule(this WebApplication app)
    {
        var swaggerOptions = app.Services.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        app.UseSwagger(c =>
        {
            c.RouteTemplate = "/openapi/swagger/{documentname}/swagger.json";
        });
        app.UseSwaggerUI(c =>
        {
            swaggerOptions.SwaggerGeneratorOptions.SwaggerDocs.ForEach(doc =>
            {
                c.SwaggerEndpoint($"/openapi/swagger/{doc.Key}/swagger.json", doc.Value.Title);
                c.RoutePrefix = "openapi";
            });
        });
    }
}
