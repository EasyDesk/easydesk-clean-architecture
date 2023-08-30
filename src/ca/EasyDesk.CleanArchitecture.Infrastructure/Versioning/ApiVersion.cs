using EasyDesk.Commons.Comparers;

namespace EasyDesk.CleanArchitecture.Infrastructure.Versioning;

public record ApiVersion(uint Major, uint Minor = 0) : IComparable<ApiVersion>
{
    private static readonly IComparer<ApiVersion> _comparer = ComparisonUtils
        .ComparerBy<ApiVersion, uint>(v => v.Major)
        .ThenBy(v => v.Minor);

    public string ToStringWithoutV() => $"{Major}.{Minor}";

    public override string ToString() => $"v{ToStringWithoutV()}";

    public int CompareTo(ApiVersion? other) => other is null ? 1 : _comparer.Compare(this, other);
}
