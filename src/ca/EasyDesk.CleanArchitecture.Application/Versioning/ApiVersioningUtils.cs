using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Application.Versioning;

public static class ApiVersioningUtils
{
    public static ApiVersion DefaultVersion => new(1, 0);

    public static Option<ApiVersion> GetApiVersionFromNamespace(this Type type)
    {
        return type.Namespace
            .AsOption()
            .FlatMap(n => n
                .Split('.')
                .Reverse()
                .SelectMany(v => ParseVersionFromNamespace(v))
                .FirstOption());
    }

    public static IFixedSet<ApiVersion> GetSupportedApiVersionsFromNamespaces(this IEnumerable<Type> types) =>
        types.SelectMany(t => t.GetApiVersionFromNamespace()).ToFixedSet();

    private static Option<ApiVersion> ParseVersionFromNamespace(string version)
    {
        var match = ApiVersionNamespaceRegex.Instance().Match(version);
        if (!match.Success)
        {
            return None;
        }
        var major = uint.Parse(match.Groups[1].Value);
        var minor = uint.Parse(match.Groups[2].Value);
        return Some(new ApiVersion(major, minor));
    }
}

public static partial class ApiVersionNamespaceRegex
{
    [GeneratedRegex(@"^V_(\d+)_(\d+)$")]
    public static partial Regex Instance();
}
