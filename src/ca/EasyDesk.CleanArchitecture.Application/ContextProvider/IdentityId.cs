using EasyDesk.Commons.Values;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record IdentityId : IValue<IdentityId, string>
{
    public const int MaxLength = 1024;

    private IdentityId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static implicit operator string(IdentityId identityId) => identityId.Value;

    public static Option<IdentityId> TryNew(string value) =>
        value.Length > MaxLength
            ? None
            : Some(new IdentityId(value));

    public static IdentityId New(string value) => TryNew(value)
        .OrElseThrow(() => new ArgumentException($"Identity id length must be less than or equal to {MaxLength}.", nameof(value)));

    public override string ToString() => Value;

    public static IdentityId FromGuid(Guid value) => New(value.ToString());

    public static IdentityId FromRandomGuid() => FromGuid(Guid.NewGuid());
}
