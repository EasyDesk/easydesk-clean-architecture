using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;
using EasyDesk.Commons.Options;
using FluentValidation;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public record TenantId : PureValue<string, TenantId>, IValue<string>
{
    public const int MaxLength = 256;

    private TenantId(string value, bool validate) : base(value, validate)
    {
    }

    public TenantId(string value) : base(value)
    {
    }

    public static TenantId FromGuid(Guid value) => new(value.ToString(), validate: false);

    public static TenantId FromRandomGuid() => FromGuid(Guid.NewGuid());

    public static Option<TenantId> TryCreate(string value) =>
        IValue<string>.Companion<TenantId>.ValidateToOption(value)
            .Map(id => new TenantId(value, validate: false));

    public static IRuleBuilder<X, string> ValidationRules<X>(IRuleBuilder<X, string> rules) => rules
        .NotEmptyOrWhiteSpace()
        .MaximumLength(MaxLength)
        .DependentRules(() => rules.Matches(TenantIdRegex.Instance()));
}

public static partial class TenantIdRegex
{
    [GeneratedRegex("^([a-zA-Z0-9_-])*$")]
    public static partial Regex Instance();
}
