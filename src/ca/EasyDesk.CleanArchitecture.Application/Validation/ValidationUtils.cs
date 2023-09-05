using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Results;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public static class ValidationUtils
{
    public static Result<T> Validate<T>(T value, IEnumerable<IValidator<T>> validators)
    {
        var errors = validators
            .Select(x => x.Validate(value))
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .Select(x => Errors.InvalidInput(x.PropertyName, x.ErrorMessage))
            .ToList();
        return errors.Any()
            ? Errors.Multiple(errors.First(), errors.Skip(1))
            : value;
    }

    public static Result<T> Validate<T>(T value, params IValidator<T>[] validators)
    {
        return Validate(value, validators.AsEnumerable());
    }
}
