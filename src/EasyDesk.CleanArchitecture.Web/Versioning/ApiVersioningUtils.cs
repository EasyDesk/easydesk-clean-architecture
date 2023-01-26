using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Web.Versioning;

public static partial class ApiVersioningUtils
{
    public const string VersionHeader = "api-version";
    public const string VersionQueryParam = "version";

    public static ApiVersion DefaultVersion => new(1, 0);

    public static Option<ApiVersion> GetControllerVersion(this Type controllerType)
    {
        return controllerType.Namespace
            .AsOption()
            .FlatMap(n => n
                .Split('.')
                .Reverse()
                .SelectMany(v => ParseVersionFromNamespace(v))
                .FirstOption());
    }

    public static Option<ApiVersion> ParseVersionFromNamespace(string version)
    {
        var match = ApiVersionNamespaceRegex().Match(version);
        if (!match.Success)
        {
            return None;
        }
        else
        {
            var major = int.Parse(match.Groups[1].Value);
            var minor = int.Parse(match.Groups[2].Value);
            return Some(new ApiVersion(major, minor));
        }
    }

    public static IEnumerable<ApiVersion> GetSupportedVersions(AppDescription app) => new AssemblyScanner()
        .FromAssemblies(app.GetLayerAssembly(CleanArchitectureLayer.Web))
        .NonAbstract()
        .SubtypesOrImplementationsOf<AbstractController>()
        .FindTypes()
        .SelectMany(t => t.GetControllerVersion())
        .Order()
        .Distinct();

    public static string ToDisplayString(this ApiVersion apiVersion)
    {
        return $"v{apiVersion}";
    }

    [GeneratedRegex("^V_(\\d+)_(\\d+)$")]
    private static partial Regex ApiVersionNamespaceRegex();
}
