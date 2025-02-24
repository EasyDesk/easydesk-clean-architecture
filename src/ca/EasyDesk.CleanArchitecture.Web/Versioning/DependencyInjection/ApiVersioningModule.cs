using Asp.Versioning;
using Asp.Versioning.Conventions;
using Autofac;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
        var supportedVersions = new AssemblyScanner()
            .FromAssemblies(app.Assemblies)
            .NonAbstract()
            .SubtypesOrImplementationsOf<AbstractController>()
            .FindTypes()
            .GetSupportedApiVersionsFromNamespaces();
        ApiVersioningInfo = new ApiVersioningInfo(supportedVersions);
    }

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterInstance(ApiVersioningInfo!)
            .SingleInstance();
    }

    protected override void ConfigureServices(AppDescription app, IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;

                options.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader(RestApiVersioning.VersionQueryParam),
                    new HeaderApiVersionReader(RestApiVersioning.VersionHeader));

                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = ApiVersioningInfo!.SupportedVersions
                    .MaxOption()
                    .OrElseGet(() => ApiVersioningUtils.DefaultVersion)
                    .ToAspNetApiVersion();

                _configure?.Invoke(options);
            })
            .AddMvc(options =>
            {
                options.Conventions.Add(new NamespaceConvention());
            });
    }

    private class NamespaceConvention : IControllerConvention
    {
        public bool Apply(IControllerConventionBuilder controller, ControllerModel controllerModel)
        {
            controllerModel.ControllerType
                .GetApiVersionFromNamespace()
                .Map(v => v.ToAspNetApiVersion())
                .Match(
                    some: controller.HasApiVersion,
                    none: controller.IsApiVersionNeutral);
            return true;
        }
    }
}

public static class ApiVersioningModuleExtensions
{
    public static IAppBuilder AddApiVersioning(this IAppBuilder builder, Action<ApiVersioningOptions>? configure = null)
    {
        return builder.AddModule(new ApiVersioningModule(configure));
    }
}
