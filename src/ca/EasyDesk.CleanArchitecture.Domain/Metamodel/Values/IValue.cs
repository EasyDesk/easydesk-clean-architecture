using EasyDesk.Commons.Options;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

public interface IValue<T> where T : notnull
{
    static abstract IRuleBuilder<X, T> ValidationRules<X>(IRuleBuilder<X, T> rules);

    public static class Companion<TSelf>
        where TSelf : IValue<T>
    {
        private static readonly IValidator<T> _processedValueValidator;

        static Companion()
        {
            var validator = new InlineValidator<T>();
            TSelf.ValidationRules(validator.RuleFor(x => x));
            _processedValueValidator = validator;
        }

        public static T Validate(T value)
        {
            _processedValueValidator.ValidateAndThrow(value);
            return value;
        }

        public static Option<T> ValidateToOption(T value) =>
            Some(value).Filter(v => _processedValueValidator.Validate(v).IsValid);

        public static IRuleBuilder<X, T> ValidationRules<X>(IRuleBuilder<X, T> rules) => TSelf.ValidationRules(rules);
    }
}
