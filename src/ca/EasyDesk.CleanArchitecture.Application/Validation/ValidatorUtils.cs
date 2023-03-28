using FluentValidation;
using static EasyDesk.Commons.ComparisonUtils;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public static class ValidatorUtils
{
    public static IRuleBuilderOptions<T, TProperty> MustBeLessThan<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty value)
        where TProperty : IComparable<TProperty> =>
        builder.MustBeLessThan(_ => value);

    public static IRuleBuilderOptions<T, TProperty> MustBeLessThanOrEqualTo<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty value)
        where TProperty : IComparable<TProperty> =>
        builder.MustBeLessThanOrEqualTo(_ => value);

    public static IRuleBuilderOptions<T, TProperty> MustBeGreaterThan<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty value)
        where TProperty : IComparable<TProperty> =>
        builder.MustBeGreaterThan(_ => value);

    public static IRuleBuilderOptions<T, TProperty> MustBeGreaterThanOrEqualTo<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, TProperty value)
        where TProperty : IComparable<TProperty> =>
        builder.MustBeGreaterThanOrEqualTo(_ => value);

    public static IRuleBuilderOptions<T, TProperty> MustBeLessThan<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, Func<T, TProperty> value)
        where TProperty : IComparable<TProperty>
    {
        return builder
            .Must(Be(LessThan, value))
            .WithMessage(t => $"{{PropertyName}} must be less than {value(t)}");
    }

    public static IRuleBuilderOptions<T, TProperty> MustBeLessThanOrEqualTo<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, Func<T, TProperty> value)
        where TProperty : IComparable<TProperty>
    {
        return builder
            .Must(Be(LessThanOrEqualTo, value))
            .WithMessage(t => $"{{PropertyName}} must be less than or equal to {value(t)}");
    }

    public static IRuleBuilderOptions<T, TProperty> MustBeGreaterThan<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, Func<T, TProperty> value)
        where TProperty : IComparable<TProperty>
    {
        return builder
            .Must(Be(GreaterThan, value))
            .WithMessage(t => $"{{PropertyName}} must be greater than {value(t)}");
    }

    public static IRuleBuilderOptions<T, TProperty> MustBeGreaterThanOrEqualTo<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder, Func<T, TProperty> value)
        where TProperty : IComparable<TProperty>
    {
        return builder
            .Must(Be(GreaterThanOrEqualTo, value))
            .WithMessage(t => $"{{PropertyName}} must be greater than or equal to {value(t)}");
    }

    private static Func<T, TProperty, bool> Be<T, TProperty>(Predicate<int> comparison, Func<T, TProperty> value)
        where TProperty : IComparable<TProperty> => (t, p) =>
        {
            var valueToCompare = value(t);
            if (valueToCompare is null)
            {
                return true;
            }
            var compareResult = p?.CompareTo(valueToCompare);
            return compareResult is null || comparison(compareResult.Value);
        };
}
