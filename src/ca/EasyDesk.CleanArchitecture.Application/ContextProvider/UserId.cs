using EasyDesk.Commons.Values;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record UserId : IValue<UserId, string>
{
    public const int MaxLength = 1024;

    private UserId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static implicit operator string(UserId userId) => userId.Value;

    public static Option<UserId> TryNew(string value) =>
        value.Length > MaxLength
            ? None
            : Some(new UserId(value));

    public static UserId New(string value) => TryNew(value)
        .OrElseThrow(() => new ArgumentException($"User id length must be less than or equal to {MaxLength}.", nameof(value)));

    public override string ToString() => Value;

    public static UserId FromGuid(Guid value) => New(value.ToString());

    public static UserId FromRandomGuid() => FromGuid(Guid.NewGuid());
}
