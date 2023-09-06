using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Domain.Model;

public record Email : PureValue<string, Email>, IValue<string>
{
    public const int MaxLength = 254;

    public Email(string value) : base(value)
    {
    }

    public static IRuleBuilder<X, string> ValidationRules<X>(IRuleBuilder<X, string> rules) => rules
        .MaximumLength(MaxLength)
        .EmailAddress();
}
