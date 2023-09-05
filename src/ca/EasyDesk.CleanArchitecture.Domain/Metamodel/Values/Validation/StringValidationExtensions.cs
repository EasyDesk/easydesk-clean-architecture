using FluentValidation;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;

public static partial class StringValidationExtensions
{
    public static IRuleBuilderOptions<T, string> NotEmptyOrWhiteSpace<T>(this IRuleBuilder<T, string> rules) => rules
        .NotEmpty()
        .Must(s => !string.IsNullOrWhiteSpace(s)).WithMessage("{PropertyName} is white space.");

    public static IRuleBuilderOptions<T, string> AlphanumericCode<T>(this IRuleBuilder<T, string> rules) =>
        rules.Matches(AlphanumericCodeRegex());

    [GeneratedRegex(@"^\w*$")]
    public static partial Regex AlphanumericCodeRegex();
}
