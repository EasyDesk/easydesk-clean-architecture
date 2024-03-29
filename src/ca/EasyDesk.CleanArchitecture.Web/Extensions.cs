﻿using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.DomainServices.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.ErrorManagement.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Validation.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;
using EasyDesk.Commons.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Web;

public static partial class Extensions
{
    private static readonly string[] _layerNames =
    [
        "Web",
        "Infrastructure",
        "Application",
        "Domain",
    ];

    /// <summary>
    /// Configures the <see cref="WebApplicationBuilder"/> to use the clean architecture framework.
    /// Below the list of modules added by default using this method:
    /// <list type="bullet">
    ///     <item><see cref="ControllersModule"/></item>
    ///     <item><see cref="JsonModule"/></item>
    ///     <item><see cref="DomainLayerModule"/></item>
    ///     <item><see cref="ContextProviderModule"/></item>
    ///     <item><see cref="TimeManagementModule"/></item>
    ///     <item><see cref="DispatchingModule"/></item>
    ///     <item><see cref="MultitenancyModule"/></item>
    ///     <item><see cref="ValidationModule"/></item>
    /// </list>
    /// Additional modules can be added using the configuration action passed when calling this method.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <param name="configure">Additional configuration for the <see cref="AppBuilder"/>.</param>
    /// <returns>The <see cref="AppDescription"/> created after configuration.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static AppDescription ConfigureForCleanArchitecture(
        this WebApplicationBuilder builder,
        Action<AppBuilder>? configure = null)
    {
        var callingAssemblyName = Assembly.GetCallingAssembly().GetName().Name ?? throw new InvalidOperationException("Calling assembly name not found.");
        var match = WebAssemblyRegex().Match(callingAssemblyName);
        var assemblyPrefix = match.Success ? match.Groups[1].Value : callingAssemblyName;
        var assemblies = _layerNames.SelectMany(layer => LoadAssemblyIfPresent($"{assemblyPrefix}.{layer}"));

        var appBuilder = new AppBuilder(assemblyPrefix)
            .WithAssemblies(assemblies)
            .AddControllers(builder.Environment)
            .AddJsonSerialization()
            .AddDomainLayer()
            .AddContextProvider(builder.Environment)
            .AddTimeManagement()
            .AddDispatching()
            .AddValidation()
            .AddErrorManagement();

        configure?.Invoke(appBuilder);

        var appDescription = appBuilder.Build();

        appDescription.ConfigureServices(builder.Services);

        return appDescription;
    }

    private static Option<Assembly> LoadAssemblyIfPresent(string name)
    {
        try
        {
            return Some(Assembly.Load(name));
        }
        catch
        {
            return None;
        }
    }

    [GeneratedRegex("^(.+)\\.Web$")]
    private static partial Regex WebAssemblyRegex();
}
