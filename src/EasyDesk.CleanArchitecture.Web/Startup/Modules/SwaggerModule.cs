using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Web.Swagger;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Web.Startup.Modules;

public class SwaggerModule : IAppModule
{
    private readonly Action<SwaggerGenOptions> _configure;

    public SwaggerModule(Action<SwaggerGenOptions> configure = null)
    {
        _configure = configure;
    }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddSwaggerGenNewtonsoftSupport();
        services.AddSwaggerGen(options =>
        {
            VersioningUtils.GetSupportedVersions(app.WebAssemblyMarker)
                .Select(version => version.ToDisplayString())
                .ForEach(version =>
                {
                    options.SwaggerDoc(version, new OpenApiInfo
                    {
                        Title = app.Name,
                        Version = version
                    });
                });

            options.OperationFilter<ApiVersionFilter>();
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

            app.GetModule<AuthenticationModule>().IfPresent(auth =>
            {
                auth.Schemes.ForEach(scheme => scheme.Value.ConfigureSwagger(options));
            });

            _configure?.Invoke(options);
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
