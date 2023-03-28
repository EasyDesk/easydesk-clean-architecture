using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Infrastructure.Configuration;

public static partial class ApiVersionNamespaceRegexConfiguration
{
    [GeneratedRegex("^V_(\\d+)_(\\d+)$")]
    public static partial Regex ApiVersionNamespaceRegex();
}
