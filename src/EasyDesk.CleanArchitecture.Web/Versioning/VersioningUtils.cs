using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Web.Versioning;

public static class VersioningUtils
{
    public const string VersionHeader = "api-version";
    public const string VersionQueryParam = "version";

    public static ApiVersion DefaultVersion => new(1, 0);

    public static Option<ApiVersion> GetControllerVersion(this Type controllerType)
    {
        return controllerType.Namespace
            .Split('.')
            .Reverse()
            .SelectMany(v => ParseVersionFromNamespace(v))
            .FirstOption();
    }

    public static Option<ApiVersion> ParseVersionFromNamespace(string version)
    {
        var match = Regex.Match(version, @"^V_(\d+)_(\d+)$");
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
        .OrderBy(x => x)
        .Distinct();

    public static string ToDisplayString(this ApiVersion apiVersion)
    {
        return $"v{apiVersion}";
    }
}
