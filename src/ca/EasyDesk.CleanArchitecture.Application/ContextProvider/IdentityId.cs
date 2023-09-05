using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record IdentityId : PureValue<string, IdentityId>, IValue<string>
{
    public const int MaxLength = 1024;

    public IdentityId(string value) : base(value)
    {
    }

    public static IdentityId FromGuid(Guid value) => new(value.ToString());

    public static IdentityId FromRandomGuid() => FromGuid(Guid.NewGuid());

    public static IRuleBuilder<X, string> Validate<X>(IRuleBuilder<X, string> rules) =>
        rules.NotEmptyOrWhiteSpace().MaximumLength(MaxLength);
}
