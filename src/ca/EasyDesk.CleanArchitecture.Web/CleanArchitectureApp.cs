using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.DomainServices.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.ErrorManagement.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Validation.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Context.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.AppBuilders;
using EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Web;

public static partial class CleanArchitectureApp
{
    private const string ArgsSeparator = "---";

    private static readonly string[] _layerNames =
    [
        "Web",
        "Infrastructure",
        "Application",
        "Domain",
    ];

    public static CleanArchitectureWebAppBuilder CreateWebAppBuilder(string[] args, Assembly? assembly = default)
    {
        var callingAssemblyName = (assembly ?? Assembly.GetCallingAssembly())?.GetName().Name ?? throw new InvalidOperationException("Calling assembly name not found.");
        var match = WebAssemblyRegex().Match(callingAssemblyName);
        var assemblyPrefix = match.Success ? match.Groups[1].Value : callingAssemblyName;
        var assemblies = _layerNames.SelectMany(layer => LoadAssemblyIfPresent($"{assemblyPrefix}.{layer}"));

        var (commandArgs, configurationArgs) = SplitArgs(args);
        var builder = new CleanArchitectureWebAppBuilder(assemblyPrefix, commandArgs, configurationArgs);
        builder.WithAssemblies(assemblies);
        return builder;
    }

    public static CleanArchitectureHostBuilder CreateHostBuilder(string name, string[] args, Assembly? assembly = default)
    {
        var (commandArgs, configurationArgs) = SplitArgs(args);
        var builder = new CleanArchitectureHostBuilder(name, commandArgs, configurationArgs);
        builder.WithAssemblies([assembly ?? Assembly.GetCallingAssembly(),]);
        return builder;
    }

    public static CleanArchitectureWebAppBuilder ConfigureDefaults(this CleanArchitectureWebAppBuilder builder)
    {
        builder.AddControllers(options => options.HideUnhandledExceptions = !builder.Environment.IsDevelopment());
        builder.ConfigureCommonDefaults();
        return builder;
    }

    public static CleanArchitectureHostBuilder ConfigureDefaults(this CleanArchitectureHostBuilder builder)
    {
        builder.ConfigureCommonDefaults();
        return builder;
    }

    private static B ConfigureCommonDefaults<B>(this B builder) where B : CleanArchitectureAppBuilder
    {
        builder
            .AddJsonSerialization()
            .AddDomainLayer()
            .AddContextDetector()
            .AddTimeManagement()
            .AddDispatching()
            .AddValidation()
            .AddErrorManagement();
        return builder;
    }

    private static (string[] CommandArgs, string[] ConfigurationArgs) SplitArgs(string[] args)
    {
        if (args is [var firstArg, ..] && IsConfigurationArg(firstArg))
        {
            return ([], args);
        }

        var index = Array.IndexOf(args, ArgsSeparator);
        return index >= 0 ? (args[0..index], args[(index + 1)..]) : (args, []);
    }

    private static bool IsConfigurationArg(string arg)
    {
        return CommandLineConfigurationRegex().Match(arg).Success;
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

    [GeneratedRegex("^-|^/|.=")]
    private static partial Regex CommandLineConfigurationRegex();
}
