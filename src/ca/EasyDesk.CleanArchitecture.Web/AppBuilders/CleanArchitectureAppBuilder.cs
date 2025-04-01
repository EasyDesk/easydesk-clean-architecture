using Autofac;
using Autofac.Extensions.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace EasyDesk.CleanArchitecture.Web.AppBuilders;

public abstract class CleanArchitectureAppBuilder : AppBuilder, IAppBuilder
{
    protected CleanArchitectureAppBuilder(string name) : base(name)
    {
    }

    public abstract IConfigurationManager Configuration { get; }

    public abstract IHostEnvironment Environment { get; }

    public abstract ILoggingBuilder Logging { get; }

    public abstract IMetricsBuilder Metrics { get; }
}

public abstract class CleanArchitectureAppBuilder<B, H> : CleanArchitectureAppBuilder
    where B : IHostApplicationBuilder
    where H : IHost
{
    private Action<H>? _configureApplication;
    private readonly string[] _args;

    protected CleanArchitectureAppBuilder(string name, string[] commandArgs, B builder) : base(name)
    {
        _args = commandArgs;
        Builder = builder;
    }

    public override IConfigurationManager Configuration => Builder.Configuration;

    public override IHostEnvironment Environment => Builder.Environment;

    public override ILoggingBuilder Logging => Builder.Logging;

    public override IMetricsBuilder Metrics => Builder.Metrics;

    protected B Builder { get; }

    public CleanArchitectureAppBuilder<B, H> ConfigureApplication(Action<H> configure)
    {
        _configureApplication += configure;
        return this;
    }

    private H Build()
    {
        var appDescription = BuildAppDescription();

        Builder.Services.AddSingleton(appDescription);

        var serviceRegistry = new ServiceRegistry();
        appDescription.ConfigureRegistry(serviceRegistry);

        appDescription.BeforeServiceConfiguration();

        serviceRegistry.ApplyToServiceCollection(Builder.Services);

        Builder.ConfigureContainer(new AutofacServiceProviderFactory(), serviceRegistry.ApplyToContainerBuilder);

        var app = BuildHost(Builder);

        _configureApplication?.Invoke(app);

        return app;
    }

    protected abstract H BuildHost(B builder);

    public override async Task<int> Run()
    {
        var app = Build();

        await using var scope = app.Services.GetRequiredService<ILifetimeScope>().BeginLifetimeScope();

        var commands = scope.Resolve<IEnumerable<Command>>();

        var rootCommand = CreateRootCommand(app);

        commands.ForEach(rootCommand.Add);

        return await rootCommand.InvokeAsync(_args);
    }

    private RootCommand CreateRootCommand(H app)
    {
        var command = new RootCommand("Start the service");
        command.SetHandler(app.Run);
        return command;
    }
}
