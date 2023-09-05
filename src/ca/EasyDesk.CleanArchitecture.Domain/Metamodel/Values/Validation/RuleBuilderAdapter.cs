using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;

internal class RuleBuilderAdapter<T, P, R> : IRuleBuilder<T, R>
{
    private readonly IRuleBuilder<T, P> _wrapped;
    private readonly Func<P, R> _mapping;

    public RuleBuilderAdapter(IRuleBuilder<T, P> rules, Func<P, R> mapping)
    {
        _wrapped = rules;
        _mapping = mapping;
    }

    public IRuleBuilderOptions<T, R> SetAsyncValidator(IAsyncPropertyValidator<T, R> validator)
    {
        return new RuleBuilderOptionsAdapter(_wrapped.SetAsyncValidator(new AsyncPropertyValidatorAdapter(validator, _mapping)), _mapping);
    }

    public IRuleBuilderOptions<T, R> SetValidator(IPropertyValidator<T, R> validator)
    {
        return new RuleBuilderOptionsAdapter(_wrapped.SetValidator(new PropertyValidatorAdapter(validator, _mapping)), _mapping);
    }

    public IRuleBuilderOptions<T, R> SetValidator(IValidator<R> validator, params string[] ruleSets)
    {
        return new RuleBuilderOptionsAdapter(_wrapped.SetValidator(new ValidatorAdapter(validator, _mapping), ruleSets), _mapping);
    }

    public IRuleBuilderOptions<T, R> SetValidator<TValidator>(Func<T, TValidator> validatorProvider, params string[] ruleSets) where TValidator : IValidator<R>
    {
        return new RuleBuilderOptionsAdapter(_wrapped.SetValidator(t => new ValidatorAdapter(validatorProvider(t), _mapping), ruleSets), _mapping);
    }

    public IRuleBuilderOptions<T, R> SetValidator<TValidator>(Func<T, R, TValidator> validatorProvider, params string[] ruleSets) where TValidator : IValidator<R>
    {
        return new RuleBuilderOptionsAdapter(_wrapped.SetValidator((t, r) => new ValidatorAdapter(validatorProvider(t, _mapping(r)), _mapping), ruleSets), _mapping);
    }

    private class AsyncPropertyValidatorAdapter : IAsyncPropertyValidator<T, P>
    {
        private readonly IAsyncPropertyValidator<T, R> _wrapped;
        private readonly Func<P, R> _mapping;

        public AsyncPropertyValidatorAdapter(IAsyncPropertyValidator<T, R> wrapped, Func<P, R> mapping)
        {
            _wrapped = wrapped;
            _mapping = mapping;
        }

        public string Name => _wrapped.Name;

        public string GetDefaultMessageTemplate(string errorCode) => _wrapped.GetDefaultMessageTemplate(errorCode);

        public Task<bool> IsValidAsync(ValidationContext<T> context, P value, CancellationToken cancellation) =>
            _wrapped.IsValidAsync(context, _mapping(value), cancellation);
    }

    private class PropertyValidatorAdapter : IPropertyValidator<T, P>
    {
        private readonly IPropertyValidator<T, R> _wrapped;
        private readonly Func<P, R> _mapping;

        public PropertyValidatorAdapter(IPropertyValidator<T, R> wrapped, Func<P, R> mapping)
        {
            _wrapped = wrapped;
            _mapping = mapping;
        }

        public string Name => _wrapped.Name;

        public string GetDefaultMessageTemplate(string errorCode) => _wrapped.GetDefaultMessageTemplate(errorCode);

        public bool IsValid(ValidationContext<T> context, P value) =>
            _wrapped.IsValid(context, _mapping(value));
    }

    public class RuleBuilderOptionsAdapter : IRuleBuilderOptions<T, R>
    {
        private readonly IRuleBuilderOptions<T, P> _wrapped;
        private readonly Func<P, R> _mapping;

        public RuleBuilderOptionsAdapter(IRuleBuilderOptions<T, P> wrapped, Func<P, R> mapping)
        {
            _wrapped = wrapped;
            _mapping = mapping;
        }

        public IRuleBuilderOptions<T, R> DependentRules(Action action)
        {
            _wrapped.DependentRules(action);
            return this;
        }

        public IRuleBuilderOptions<T, R> SetAsyncValidator(IAsyncPropertyValidator<T, R> validator)
        {
            _wrapped.SetAsyncValidator(new AsyncPropertyValidatorAdapter(validator, _mapping));
            return this;
        }

        public IRuleBuilderOptions<T, R> SetValidator(IPropertyValidator<T, R> validator)
        {
            _wrapped.SetValidator(new PropertyValidatorAdapter(validator, _mapping));
            return this;
        }

        public IRuleBuilderOptions<T, R> SetValidator(IValidator<R> validator, params string[] ruleSets)
        {
            _wrapped.SetValidator(new ValidatorAdapter(validator, _mapping), ruleSets);
            return this;
        }

        public IRuleBuilderOptions<T, R> SetValidator<TValidator>(Func<T, TValidator> validatorProvider, params string[] ruleSets) where TValidator : IValidator<R>
        {
            _wrapped.SetValidator(t => new ValidatorAdapter(validatorProvider(t), _mapping), ruleSets);
            return this;
        }

        public IRuleBuilderOptions<T, R> SetValidator<TValidator>(Func<T, R, TValidator> validatorProvider, params string[] ruleSets) where TValidator : IValidator<R>
        {
            _wrapped.SetValidator((t, p) => new ValidatorAdapter(validatorProvider(t, _mapping(p)), _mapping), ruleSets);
            return this;
        }
    }

    private class ValidatorAdapter : IValidator<P>
    {
        private readonly IValidator<R> _wrapped;
        private readonly Func<P, R> _mapping;

        public ValidatorAdapter(IValidator<R> wrapped, Func<P, R> mapping)
        {
            _wrapped = wrapped;
            _mapping = mapping;
        }

        public bool CanValidateInstancesOfType(Type type) => _wrapped.CanValidateInstancesOfType(type);

        public IValidatorDescriptor CreateDescriptor() => _wrapped.CreateDescriptor();

        public ValidationResult Validate(P instance) => _wrapped.Validate(_mapping(instance));

        public ValidationResult Validate(IValidationContext context) => _wrapped.Validate(context);

        public Task<ValidationResult> ValidateAsync(P instance, CancellationToken cancellation = default) =>
            _wrapped.ValidateAsync(_mapping(instance), cancellation);

        public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = default) =>
            _wrapped.ValidateAsync(context, cancellation);
    }
}
