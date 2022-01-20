using EasyDesk.CleanArchitecture.Application.Features;
using EasyDesk.CleanArchitecture.Web.Startup.Features;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Web.Startup.Features;

public class ApiVersioningFeature : IAppFeature
{
    private readonly Action<ApiVersioningOptions> _configure;

    public ApiVersioningFeature(Action<ApiVersioningOptions> configure = null)
    {
        _configure = configure;
    }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;

            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("version"),
                new HeaderApiVersionReader("Api-Version"));

            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = VersioningUtils.GetSupportedVersions(app.WebAssemblyMarker)
                .MaxOption()
                .OrElseGet(() => VersioningUtils.DefaultVersion);

            options.Conventions.Add(new NamespaceConvention());

            _configure?.Invoke(options);
        });
    }

    private class NamespaceConvention : IControllerConvention
    {
        public bool Apply(IControllerConventionBuilder controller, ControllerModel controllerModel)
        {
            controllerModel.ControllerType.GetControllerVersion()
                .Match(
                    some: v => controller.HasApiVersion(v),
                    none: () => controller.IsApiVersionNeutral());
            return true;
        }
    }
}

public static class ApiVersioningFeatureExtensions
{
    public static AppBuilder AddApiVersioning(this AppBuilder builder, Action<ApiVersioningOptions> configure = null)
    {
        return builder.AddFeature(new ApiVersioningFeature(configure));
    }
}
