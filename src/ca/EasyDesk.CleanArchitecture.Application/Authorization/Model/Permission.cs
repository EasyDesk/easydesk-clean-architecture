using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Model;

public record Permission : PureValue<string, Permission>, IValue<string>
{
    public const int MaxLength = 100;

    public Permission(string value) : base(value)
    {
    }

    public static IRuleBuilder<X, string> ValidationRules<X>(IRuleBuilder<X, string> rules) => rules
        .NotEmptyOrWhiteSpace()
        .MaximumLength(MaxLength)
        .DependentRules(() => rules
            .AlphanumericCode());

    public static implicit operator Permission(Enum value) => new(value.ToString());
}
