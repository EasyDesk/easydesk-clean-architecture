using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.DomainServices.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.ErrorManagement.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Validation.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.AspNetCore.Builder;
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

    public static CleanArchitectureAppBuilder CreateBuilder(string[] args)
    {
        var callingAssemblyName = Assembly.GetCallingAssembly().GetName().Name ?? throw new InvalidOperationException("Calling assembly name not found.");
        var match = WebAssemblyRegex().Match(callingAssemblyName);
        var assemblyPrefix = match.Success ? match.Groups[1].Value : callingAssemblyName;
        var assemblies = _layerNames.SelectMany(layer => LoadAssemblyIfPresent($"{assemblyPrefix}.{layer}"));

        var (commandArgs, configurationArgs) = SplitArgs(args);
        var builder = WebApplication.CreateBuilder(configurationArgs);

        return new CleanArchitectureAppBuilder(assemblyPrefix, commandArgs, builder)
            .Also(x => x
                .WithAssemblies(assemblies)
                .AddControllers(builder.Environment)
                .AddJsonSerialization()
                .AddDomainLayer()
                .AddContextProvider(builder.Environment)
                .AddTimeManagement()
                .AddDispatching()
                .AddValidation()
                .AddErrorManagement());
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
