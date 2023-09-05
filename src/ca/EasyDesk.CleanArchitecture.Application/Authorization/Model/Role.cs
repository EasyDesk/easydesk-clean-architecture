using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Model;

public record Role : PureValue<string, Role>, IValue<string>
{
    public const int MaxLength = 100;

    public Role(string value) : base(value)
    {
    }

    public static IRuleBuilder<X, string> Validate<X>(IRuleBuilder<X, string> rules) =>
        rules.MaximumLength(MaxLength).NotEmptyOrWhiteSpace();

    public static implicit operator Role(Enum value) => new(value.ToString());
}
