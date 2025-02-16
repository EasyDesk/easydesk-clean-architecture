﻿using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
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

    internal CleanArchitectureAppBuilder(string name, string[] args, WebApplicationBuilder applicationBuilder)
        : base(name)
    {
        _args = args;
        _applicationBuilder = applicationBuilder;
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

    public override async Task<int> Run()
    {
        var appDescription = BuildAppDescription();

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
