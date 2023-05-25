using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public abstract class PimpedAbstractValidator<T> : AbstractValidator<T>
{
    public IRuleBuilderInitial<T, TProperty> RuleForOption<TProperty>(Expression<Func<T, Option<TProperty>>> property)
    {
        // Passing an empty inline validator to force a conversion to IRuleBuilderOptions<T, Option<TProperty>>.
        return new RuleBuilder<TProperty>(RuleFor(property).SetValidator(new InlineValidator<Option<TProperty>>()));
    }

    private abstract class AbstractPropertyValidator<TValidator> : IPropertyValidator
        where TValidator : IPropertyValidator
    {
        public AbstractPropertyValidator(TValidator innerValidator)
        {
            InnerValidator = innerValidator;
        }

        protected TValidator InnerValidator { get; }

        public string GetDefaultMessageTemplate(string errorCode) => InnerValidator.GetDefaultMessageTemplate(errorCode);

        public string Name => InnerValidator.Name;
    }

    private class PropertyValidator<TProperty> : AbstractPropertyValidator<IPropertyValidator<T, TProperty>>, IPropertyValidator<T, Option<TProperty>>
    {
        public PropertyValidator(IPropertyValidator<T, TProperty> innerValidator) : base(innerValidator)
        {
        }

        public bool IsValid(ValidationContext<T> context, Option<TProperty> value) =>
            value.Match(
                some: x => InnerValidator.IsValid(context, x),
                none: () => true);
    }

    private class AsyncPropertyValidator<TProperty> : AbstractPropertyValidator<IAsyncPropertyValidator<T, TProperty>>, IAsyncPropertyValidator<T, Option<TProperty>>
    {
        public AsyncPropertyValidator(IAsyncPropertyValidator<T, TProperty> innerValidator) : base(innerValidator)
        {
        }

        public Task<bool> IsValidAsync(ValidationContext<T> context, Option<TProperty> value, CancellationToken cancellation) =>
            value.Match(
                some: x => InnerValidator.IsValidAsync(context, x, cancellation),
                none: () => Task.FromResult(true));
    }

    private class Validator<TProperty> : IValidator<Option<TProperty>>
    {
        private readonly IValidator<TProperty> _innerValidator;

        public Validator(IValidator<TProperty> innerValidator)
        {
            _innerValidator = innerValidator;
        }

        private static ValidationResult NoErrors => new();

        public ValidationResult Validate(Option<TProperty> instance) =>
            instance.Match(
                some: t => _innerValidator.Validate(t),
                none: () => NoErrors);

        public Task<ValidationResult> ValidateAsync(Option<TProperty> instance, CancellationToken cancellation = default) =>
            instance.Match(
                some: t => _innerValidator.ValidateAsync(t, cancellation),
                none: () => Task.FromResult(NoErrors));

        public ValidationResult Validate(IValidationContext context)
        {
            var genericContext = ValidationContext<Option<TProperty>>.GetFromNonGenericContext(context);
            return genericContext.InstanceToValidate.Match(
                some: t => _innerValidator.Validate(genericContext.CloneForChildValidator(t)),
                none: () => NoErrors);
        }

        public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = default)
        {
            var genericContext = ValidationContext<Option<TProperty>>.GetFromNonGenericContext(context);
            return genericContext.InstanceToValidate.Match(
                some: t => _innerValidator.ValidateAsync(genericContext.CloneForChildValidator(t), cancellation),
                none: () => Task.FromResult(NoErrors));
        }

        public IValidatorDescriptor CreateDescriptor() => _innerValidator.CreateDescriptor();

        public bool CanValidateInstancesOfType(Type type)
        {
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Option<>))
            {
                return false;
            }
            return _innerValidator.CanValidateInstancesOfType(type.GetGenericArguments()[0]);
        }
    }

    private class RuleBuilder<TProperty> : IRuleBuilderOptions<T, TProperty>, IRuleBuilderInitial<T, TProperty>
    {
        private readonly IRuleBuilderOptions<T, Option<TProperty>> _innerRuleBuilder;

        public RuleBuilder(IRuleBuilderOptions<T, Option<TProperty>> innerRuleBuilder)
        {
            _innerRuleBuilder = innerRuleBuilder;
        }

        public IRuleBuilderOptions<T, TProperty> SetValidator(IPropertyValidator<T, TProperty> validator)
        {
            _innerRuleBuilder.SetValidator(new PropertyValidator<TProperty>(validator));
            return this;
        }

        public IRuleBuilderOptions<T, TProperty> SetAsyncValidator(IAsyncPropertyValidator<T, TProperty> validator)
        {
            _innerRuleBuilder.SetAsyncValidator(new AsyncPropertyValidator<TProperty>(validator));
            return this;
        }

        public IRuleBuilderOptions<T, TProperty> SetValidator(IValidator<TProperty> validator, params string[] ruleSets)
        {
            _innerRuleBuilder.SetValidator(new Validator<TProperty>(validator), ruleSets);
            return this;
        }

        public IRuleBuilderOptions<T, TProperty> SetValidator<TValidator>(Func<T, TValidator> validatorProvider, params string[] ruleSets) where TValidator : IValidator<TProperty>
        {
            _innerRuleBuilder.SetValidator(t => new Validator<TProperty>(validatorProvider(t)), ruleSets);
            return this;
        }

        public IRuleBuilderOptions<T, TProperty> SetValidator<TValidator>(Func<T, TProperty, TValidator> validatorProvider, params string[] ruleSets) where TValidator : IValidator<TProperty>
        {
            _innerRuleBuilder.SetValidator(
                (t, p) => p.Match<IValidator<Option<TProperty>>>(
                    some: pp => new Validator<TProperty>(validatorProvider(t, pp)),
                    none: () => new InlineValidator<Option<TProperty>>()),
                ruleSets);
            return this;
        }

        public IRuleBuilderOptions<T, TProperty> DependentRules(Action action)
        {
            _innerRuleBuilder.DependentRules(action);
            return this;
        }
    }
}
