using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Domain.Model;

public record Name : PureValue<string, Name>, IValue<string>
{
    public Name(string value) : base(value)
    {
    }

    public static IRuleBuilder<X, string> ValidationRules<X>(IRuleBuilder<X, string> rules) => rules.NotEmptyOrWhiteSpace();
}
