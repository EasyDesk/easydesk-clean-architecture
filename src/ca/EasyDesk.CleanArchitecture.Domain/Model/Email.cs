using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using FluentValidation;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Domain.Model;

public partial record Email : PureValue<string, Email>, IValue<string>
{
    public const int MaxLength = 254;

    public Email(string value) : base(value)
    {
    }

    public static IRuleBuilder<X, string> ValidationRules<X>(IRuleBuilder<X, string> rules) => rules
        .MaximumLength(MaxLength)
        .DependentRules(() => rules.Matches(Regex()));

    [GeneratedRegex("^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])$", RegexOptions.IgnoreCase)]
    public static partial Regex Regex();
}
