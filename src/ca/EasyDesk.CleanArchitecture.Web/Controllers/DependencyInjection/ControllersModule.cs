using Autofac;
using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;

public sealed class ControllersModuleOptions
{
    public const int DefaultDefaultPageSize = 100;
    public const int DefaultMaxPageSize = 100;

    private Action<MvcOptions>? _configureMvc;

    public int DefaultPageSize { get; set; } = DefaultDefaultPageSize;

    public int MaxPageSize { get; set; } = DefaultMaxPageSize;

    public bool HideUnhandledExceptions { get; set; } = true;

    public void ConfigureMvc(Action<MvcOptions> configureMvc) => _configureMvc += configureMvc;

    internal void ApplyMvcConfiguration(MvcOptions options) => _configureMvc?.Invoke(options);
}

public class ControllersModule : AppModule
{
    public ControllersModuleOptions Options { get; } = new();

    public ControllersModule(Action<ControllersModuleOptions>? configure = null)
    {
        configure?.Invoke(Options);
    }

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterInstance(new PaginationService(Options.DefaultPageSize, Options.MaxPageSize))
            .SingleInstance();
    }

    protected override void ConfigureServices(AppDescription app, IServiceCollection services)
    {
        services
            .AddControllers(DefaultMvcConfiguration)
            .AddApplicationPart(typeof(CleanArchitectureController).Assembly);

        services.AddSingleton<IConfigureOptions<JsonOptions>, ConfigureJsonOptions>();

        services.Configure<MvcOptions>(options =>
        {
            var formatter = options.OutputFormatters.OfType<StringOutputFormatter>().Single();
            options.OutputFormatters.Remove(formatter);
            options.OutputFormatters.Add(formatter);
        });
    }

    protected void DefaultMvcConfiguration(MvcOptions options)
    {
        options.Filters.Add<ExceptionLoggingFilter>();
        if (Options.HideUnhandledExceptions)
        {
            options.Filters.Add<UnhandledExceptionsFilter>();
        }
        options.Filters.Add<BadRequestFilter>();
        options.EnableEndpointRouting = false;
        options.ModelBinderProviders.Insert(0, new OptionBinderProvider());
        Options.ApplyMvcConfiguration(options);
    }

    private class ConfigureJsonOptions : IConfigureOptions<JsonOptions>
    {
        private readonly IComponentContext _context;
        private readonly AppDescription _app;

        public ConfigureJsonOptions(IComponentContext context, AppDescription app)
        {
            _context = context;
            _app = app;
        }

        public void Configure(JsonOptions options)
        {
            _app.RequireModule<JsonModule>().ApplyJsonConfiguration(_context, options.JsonSerializerOptions, _app);
        }
    }
}

public static class ControllersModuleExtension
{
    public static IAppBuilder AddControllers(this IAppBuilder builder, Action<ControllersModuleOptions>? configure = null)
    {
        return builder.AddModule(new ControllersModule(configure));
    }

    public static IFilterMetadata Remove<TFilterType>(this FilterCollection filters) where TFilterType : IFilterMetadata
    {
        var toRemove = filters.First(f => f is TFilterType || f is TypeFilterAttribute a && a.ImplementationType == typeof(TFilterType));
        filters.Remove(toRemove);
        return toRemove;
    }
}
