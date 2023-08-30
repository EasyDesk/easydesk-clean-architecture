using EasyDesk.Commons.Options;
using EasyDesk.Commons.Values;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record Realm : IValue<Realm, string>
{
    public const int MaxLength = 1024;

    public static Realm Default { get; } = New("main");

    private Realm(string value)
    {
        Value = value;
    }

    public static Realm New(string value) => TryNew(value)
        .OrElseThrow(() => new ArgumentException($"'{value}' is not a valid Realm.", nameof(value)));

    public static Option<Realm> TryNew(string value)
    {
        return string.IsNullOrEmpty(value) || value.Length > MaxLength
            ? None
            : Some(new Realm(value));
    }

    public static implicit operator string(Realm realm) => realm.Value;

    public string Value { get; }
}
