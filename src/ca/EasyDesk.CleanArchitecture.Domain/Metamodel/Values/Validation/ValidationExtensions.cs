using FluentValidation;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;

public static partial class ValidationExtensions
{
    public static IRuleBuilder<T, R> Transform<T, P, R>(
        this IRuleBuilder<T, P> rules,
        Func<P, R> transformation) =>
        new RuleBuilderAdapter<T, P, R>(rules, transformation);

    public static ValidateStrategySelector<T, P> MustBeValid<T, P>(this IRuleBuilder<T, P> rules)
        where P : notnull, IEquatable<P> =>
        new(rules);

    public sealed class ValidateStrategySelector<T, P>
        where P : notnull, IEquatable<P>
    {
        private readonly IRuleBuilder<T, P> _rules;

        public ValidateStrategySelector(IRuleBuilder<T, P> rules)
        {
            _rules = rules;
        }

        public IRuleBuilderOptions<T, P> For<V>() where V : AbstractValue<P, V>, IValue<P> =>
            IValue<P>.Companion<V>.ProcessAndValidate(_rules);
    }
}
