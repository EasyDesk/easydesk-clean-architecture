using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record Realm : PureValue<string, Realm>, IValue<string>
{
    public const int MaxLength = 1024;

    public static Realm Default { get; } = new("main");

    public Realm(string value) : base(value)
    {
    }

    public static IRuleBuilder<X, string> Validate<X>(IRuleBuilder<X, string> rules) =>
        rules.NotEmptyOrWhiteSpace().MaximumLength(MaxLength);
}
