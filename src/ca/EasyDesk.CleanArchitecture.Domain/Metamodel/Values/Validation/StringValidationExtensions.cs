using FluentValidation;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;

public static partial class StringValidationExtensions
{
    public static IRuleBuilderOptions<T, string> NotEmptyOrWhiteSpace<T>(this IRuleBuilder<T, string> rules) => rules
        .Must(s => !string.IsNullOrWhiteSpace(s))
        .WithMessage("'{PropertyName}' is empty or white space.")
        .WithErrorCode("NotEmptyOrWhitespace");

    public static IRuleBuilderOptions<T, string> AlphanumericCode<T>(this IRuleBuilder<T, string> rules) => rules
        .Matches(AlphanumericCodeRegex())
        .WithMessage("'{PropertyName}' is not an alphanumeric code")
        .WithErrorCode("AlphanumericCode");

    [GeneratedRegex(@"^\w*$")]
    public static partial Regex AlphanumericCodeRegex();

    public static IRuleBuilderOptions<T, string> NotStartingWith<T>(this IRuleBuilder<T, string> rules, string prefix) => rules
        .Must((_, s, ctx) =>
        {
            ctx.MessageFormatter.AppendArgument("Prefix", prefix);
            return !s.StartsWith(prefix);
        })
        .WithErrorCode("NotStartingWithPrefix")
        .WithMessage("'{PropertyName}' must not start with '{Prefix}'");

    public static IRuleBuilderOptions<T, string> NotStartingWith<T>(this IRuleBuilder<T, string> rules, Func<char, bool> predicate) => rules
        .Must((_, s, ctx) => s == string.Empty || !predicate(s[0]))
        .WithErrorCode("NotStartingWithInvalidCharacters")
        .WithMessage("'{PropertyName}' must not start with invalid characters");

    public static IRuleBuilderOptions<T, string> NotEndingWith<T>(this IRuleBuilder<T, string> rules, string suffix) => rules
        .Must((_, s, ctx) =>
        {
            ctx.MessageFormatter.AppendArgument("Suffix", suffix);
            return !s.EndsWith(suffix);
        })
        .WithErrorCode("NotEndingWithSuffix")
        .WithMessage("'{PropertyName}' must not end with '{Suffix}'");

    public static IRuleBuilderOptions<T, string> NotEndingWith<T>(this IRuleBuilder<T, string> rules, Func<char, bool> predicate) => rules
        .Must((_, s, ctx) => s == string.Empty || !predicate(s[^1]))
        .WithErrorCode("NotEndingWithInvalidCharacters")
        .WithMessage("'{PropertyName}' must not end with invalid characters");

    public static IRuleBuilderOptions<T, string> NotStartingOrEndingWith<T>(this IRuleBuilder<T, string> rules, Func<char, bool> predicate) => rules
        .Must((_, s, ctx) => s == string.Empty || (!predicate(s[0]) && !predicate(s[^1])))
        .WithErrorCode("NotStartingOrEndingWithInvalidCharacters")
        .WithMessage("'{PropertyName}' must not start or end with invalid characters");

    public static IRuleBuilder<T, string> NotStartingOrEndingWithWhitespace<T>(this IRuleBuilder<T, string> rules) => rules
        .NotStartingOrEndingWith(char.IsWhiteSpace)
        .WithErrorCode("NotStartingOrEndingWithWhitespace")
        .WithMessage("'{PropertyName}' must not start or end with whitespace characters");
}
