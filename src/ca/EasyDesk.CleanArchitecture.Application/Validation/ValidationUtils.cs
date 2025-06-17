using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Strings;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public static class ValidationUtils
{
    public static Result<T> Validate<T>(T value, IEnumerable<IValidator<T>> validators)
    {
        var errors = GetValidationErrors(value, validators).ToList();
        return errors.HasAny()
            ? Errors.Multiple(errors[0], errors.Skip(1))
            : value;
    }

    public static IEnumerable<InvalidInputError> GetValidationErrors<T>(T value, IEnumerable<IValidator<T>> validators)
    {
        return validators
            .Select(x => x.Validate(value))
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .Select(x => Errors.InvalidInput(
                x.PropertyName,
                x.ErrorCode.RemoveSuffix("Validator"),
                x.ErrorMessage,
                x.FormattedMessagePlaceholderValues.ToFixedSortedMap()));
    }

    public static Result<T> Validate<T>(T value, params IValidator<T>[] validators)
    {
        return Validate(value, validators.AsEnumerable());
    }

    public static IRuleBuilderOptions<T, P> MustBeImplicitlyValid<T, P>(this IRuleBuilder<T, P> rules)
        where P : IValidate<P>
    {
        return rules.SetValidator(IValidate<P>.Validator);
    }
}
