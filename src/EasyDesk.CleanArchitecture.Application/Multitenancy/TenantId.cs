using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public record TenantId
{
    public const int MaxLength = 256;

    private TenantId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static TenantId Create(string value) =>
        TryCreate(value).OrElseThrow(() => new ArgumentException($"TenantId '{value}' has invalid format", nameof(value)));

    public static Option<TenantId> TryCreate(string value)
    {
        if (!IsValidTenantId(value))
        {
            return None;
        }

        return Some(new TenantId(value));
    }

    private static bool IsValidTenantId(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Length <= MaxLength
            && TenantIdRegex.Instance().IsMatch(value);
    }

    public static implicit operator string(TenantId tenantId) => tenantId.Value;

    public override string ToString() => Value;
}

public static partial class TenantIdRegex
{
    [GeneratedRegex("^([a-zA-Z0-9_-])*$")]
    public static partial Regex Instance();
}
