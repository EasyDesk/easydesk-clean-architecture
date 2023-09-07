using EasyDesk.Commons.Options;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

public interface IValue<T> where T : notnull
{
    static abstract IRuleBuilder<X, T> ValidationRules<X>(IRuleBuilder<X, T> rules);

    public static class Companion<TSelf>
        where TSelf : IValue<T>
    {
        private static readonly IValidator<Proxy> _processedValueValidator;

        static Companion()
        {
            var validator = new InlineValidator<Proxy>();
            TSelf.ValidationRules(validator.RuleFor(x => x.Value));
            _processedValueValidator = validator;
        }

        public static T Validate(T value)
        {
            _processedValueValidator.ValidateAndThrow(new(value));
            return value;
        }

        public static Option<T> ValidateToOption(T value) =>
            Some(value).Filter(v => _processedValueValidator.Validate(new(v)).IsValid);

        public static IRuleBuilder<X, T> ValidationRules<X>(IRuleBuilder<X, T> rules) => TSelf.ValidationRules(rules);

        private record struct Proxy(T Value);
    }
}
