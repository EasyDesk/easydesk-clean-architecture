using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using FluentValidation;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

public record AdminId : PureValue<string, AdminId>, IValue<string>
{
    private AdminId(string value) : base(value)
    {
    }

    public static AdminId From(string id) => new(id);

    public static IRuleBuilder<X, string> Validate<X>(IRuleBuilder<X, string> rules) => rules;
}
