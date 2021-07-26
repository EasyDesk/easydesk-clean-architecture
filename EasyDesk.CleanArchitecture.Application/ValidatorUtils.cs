using EasyDesk.CleanArchitecture.Application.Data;
using FluentValidation;
using System;
using System.Linq;
using static EasyDesk.Tools.Functions;

namespace EasyDesk.CleanArchitecture.Application
{
    public static class ValidatorUtils
    {
        public static IRuleBuilderOptions<T, string> EmailValid<T>(this IRuleBuilder<T, string> builder)
        {
            var emailPattern = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])";
            return builder.NotEmpty().Matches(emailPattern);
        }

        public static IRuleBuilderOptions<T, string> RoleValid<T>(this IRuleBuilder<T, string> builder)
        {
            return builder.NotEmpty().Must(t => t.All(char.IsLetter));
        }

        public static IRuleBuilderOptions<T, string> NameValid<T>(this IRuleBuilder<T, string> builder)
        {
            return builder.NotEmpty().MaximumLength(DataConstraints.NameLength);
        }

        public static IRuleBuilderOptions<T, string> PasswordValid<T>(this IRuleBuilder<T, string> builder)
        {
            return builder.NotEmpty().MinimumLength(8);
        }

        public static IRuleBuilderOptions<T, string> UsernameValid<T>(this IRuleBuilder<T, string> builder)
        {
            return builder.NotEmpty().Matches(@"^[A-Za-z][A-Za-z0-9]*(_[A-Za-z0-9]{2,}){0,3}$").MinimumLength(4);
        }

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
}
