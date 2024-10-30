using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Reflection;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web;

public sealed class CleanArchitectureAppBuilder : IAppBuilder
{
    private readonly ModulesCollection _modules = new();
    private IFixedSet<Assembly> _assemblies = Set<Assembly>();
    private string _name;
    private readonly string[] _args;
    private readonly WebApplicationBuilder _applicationBuilder;
    private Action<WebApplication>? _configureWebApplication;

    internal CleanArchitectureAppBuilder(string name, string[] args, WebApplicationBuilder applicationBuilder)
    {
        _name = name;
        _args = args;
        _applicationBuilder = applicationBuilder;
    }

    public IConfigurationManager Configuration => _applicationBuilder.Configuration;

    public IHostEnvironment Environment => _applicationBuilder.Environment;

    public ILoggingBuilder Logging => _applicationBuilder.Logging;

    public IMetricsBuilder Metrics => _applicationBuilder.Metrics;

    public IAppBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public IAppBuilder WithAssemblies(IEnumerable<Assembly> assemblies)
    {
        _assemblies = _assemblies.Union(assemblies);
        return this;
    }

    public IAppBuilder AddModule<T>(T module) where T : AppModule
    {
        _modules.AddModule(module);
        return this;
    }

    public IAppBuilder RemoveModule<T>() where T : AppModule
    {
        _modules.RemoveModule<T>();
        return this;
    }

    public IAppBuilder ConfigureModule<T>(Action<T> configure) where T : AppModule
    {
        var module = _modules
            .GetModule<T>()
            .OrElseThrow(() => new RequiredModuleMissingException(typeof(T)));
        configure(module);
        return this;
    }

    public IAppBuilder ConfigureWebApplication(Action<WebApplication> configure)
    {
        _configureWebApplication += configure;
        return this;
    }

    public async Task<int> Run()
    {
        var appDescription = new AppDescription(_name, _modules, _assemblies);

        _applicationBuilder.Services.AddSingleton(appDescription);

        appDescription.ConfigureServices(_applicationBuilder.Services);

        var app = _applicationBuilder.Build();

        _configureWebApplication?.Invoke(app);

        await using var scope = app.Services.CreateAsyncScope();

        var commands = scope.ServiceProvider.GetServices<Command>();

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
