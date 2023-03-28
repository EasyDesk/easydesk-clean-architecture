using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;

public class ApiVersioningModule : AppModule
{
    private readonly Action<ApiVersioningOptions>? _configure;

    public ApiVersioningModule(Action<ApiVersioningOptions>? configure = null)
    {
        _configure = configure;
    }

    public ApiVersioningInfo? ApiVersioningInfo { get; private set; }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        var supportedVersions = ApiVersioningUtils.GetSupportedVersions(app).ToEquatableSet();
        ApiVersioningInfo = new ApiVersioningInfo(supportedVersions);
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddSingleton(ApiVersioningInfo!);

        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;

            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader(ApiVersioningUtils.VersionQueryParam),
                new HeaderApiVersionReader(ApiVersioningUtils.VersionHeader));

            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = ApiVersioningInfo!.SupportedVersions
                .MaxOption()
                .OrElseGet(() => ApiVersioningUtils.DefaultVersion);

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

public static class ApiVersioningModuleExtensions
{
    public static AppBuilder AddApiVersioning(this AppBuilder builder, Action<ApiVersioningOptions>? configure = null)
    {
        return builder.AddModule(new ApiVersioningModule(configure));
    }
}
