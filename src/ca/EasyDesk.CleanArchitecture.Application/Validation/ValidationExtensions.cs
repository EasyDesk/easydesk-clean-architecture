using EasyDesk.Commons.Collections;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> OneOf<T, TProperty>(this IRuleBuilder<T, TProperty> builder, params IEnumerable<TProperty> values) => builder
        .Must(x => values.Contains(x))
        .WithErrorCode("OneOf")
        .WithMessage($"{{PropertyName}} must be one of {values.ToListString()}");
}
