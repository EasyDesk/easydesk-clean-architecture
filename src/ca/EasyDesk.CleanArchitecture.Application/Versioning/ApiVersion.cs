using EasyDesk.Commons.Comparers;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Strings;

namespace EasyDesk.CleanArchitecture.Application.Versioning;

public record ApiVersion(uint Major, uint Minor = 0) : IComparable<ApiVersion>
{
    private static readonly IComparer<ApiVersion> _comparer = ComparisonUtils
        .ComparerBy<ApiVersion, uint>(v => v.Major)
        .ThenBy(v => v.Minor);

    public string ToStringWithoutV() => $"{Major}.{Minor}";

    public override string ToString() => $"v{ToStringWithoutV()}";

    public int CompareTo(ApiVersion? other) => other is null ? 1 : _comparer.Compare(this, other);

    public static bool operator <(ApiVersion left, ApiVersion right) => left.CompareTo(right) < 0;

    public static bool operator >(ApiVersion left, ApiVersion right) => left.CompareTo(right) > 0;

    public static bool operator <=(ApiVersion left, ApiVersion right) => left.CompareTo(right) <= 0;

    public static bool operator >=(ApiVersion left, ApiVersion right) => left.CompareTo(right) >= 0;

    public static Option<ApiVersion> Parse(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return None;
        }
        version = version.RemovePrefix("v");
        var parts = version.Split('.');
        if (parts.Length is 0 or > 2)
        {
            return None;
        }
        if (!uint.TryParse(parts[0], out var major))
        {
            return None;
        }
        var minor = parts.Length > 1 && uint.TryParse(parts[1], out var parsedMinor) ? parsedMinor : 0;
        return Some(new ApiVersion(major, minor));
    }
}
