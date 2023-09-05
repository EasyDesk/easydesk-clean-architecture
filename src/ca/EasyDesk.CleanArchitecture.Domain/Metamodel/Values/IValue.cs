using EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;
using EasyDesk.Commons.Options;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

public interface IValue<T> where T : notnull
{
    static virtual T Process(T value) => value;

    static abstract IRuleBuilder<X, T> Validate<X>(IRuleBuilder<X, T> rules);

    public static class Companion<TSelf>
        where TSelf : IValue<T>
    {
        private static readonly IValidator<T> _rawValueValidator;

        static Companion()
        {
            var validator = new InlineValidator<T>();
            TSelf.Validate(validator.RuleFor(x => x));
            _rawValueValidator = validator;
        }

        public static T ProcessAndValidate(T value)
        {
            var processed = TSelf.Process(value);
            _rawValueValidator.ValidateAndThrow(processed);
            return processed;
        }

        public static Option<T> ProcessAndValidateToOption(T value) =>
            TSelf.Process(value).AsSome().Filter(v => _rawValueValidator.Validate(v).IsValid);

        public static IRuleBuilderOptions<X, T> ProcessAndValidate<X>(IRuleBuilder<X, T> rules) =>
            rules.Transform(TSelf.Process).SetValidator(_rawValueValidator);
    }
}
