using Autofac;
using Autofac.Extensions.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace EasyDesk.CleanArchitecture.Web;

public sealed class CleanArchitectureAppBuilder : AppBuilder, IAppBuilder
{
    private readonly string[] _args;
    private readonly WebApplicationBuilder _applicationBuilder;
    private Action<WebApplication>? _configureWebApplication;

    internal CleanArchitectureAppBuilder(string name, string[] commandArgs, string[] configurationArgs)
        : base(name)
    {
        _applicationBuilder = WebApplication.CreateBuilder(configurationArgs);
        _args = commandArgs;
    }

    public IConfigurationManager Configuration => _applicationBuilder.Configuration;

    public IHostEnvironment Environment => _applicationBuilder.Environment;

    public ILoggingBuilder Logging => _applicationBuilder.Logging;

    public IMetricsBuilder Metrics => _applicationBuilder.Metrics;

    public IAppBuilder ConfigureWebApplication(Action<WebApplication> configure)
    {
        _configureWebApplication += configure;
        return this;
    }

    public WebApplication Build()
    {
        var appDescription = BuildAppDescription();

        _applicationBuilder.Services.AddSingleton(appDescription);

        var serviceRegistry = new ServiceRegistry();
        appDescription.ConfigureRegistry(serviceRegistry);

        appDescription.BeforeServiceConfiguration();

        serviceRegistry.ApplyToServiceCollection(_applicationBuilder.Services);

        _applicationBuilder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        _applicationBuilder.Host.ConfigureContainer<ContainerBuilder>(serviceRegistry.ApplyToContainerBuilder);

        var app = _applicationBuilder.Build();

        _configureWebApplication?.Invoke(app);

        return app;
    }

    public override async Task<int> Run()
    {
        var app = Build();

        await using var scope = app.Services.GetRequiredService<ILifetimeScope>().BeginUseCaseLifetimeScope();

        var commands = scope.Resolve<IEnumerable<Command>>();

        var rootCommand = CreateRootCommand(app);

        commands.ForEach(rootCommand.Add);

        return await rootCommand.InvokeAsync(_args);
    }

    private RootCommand CreateRootCommand(WebApplication app)
    {
        var command = new RootCommand("Start the service");
        command.SetHandler(app.Run);
        return command;
    }
}
