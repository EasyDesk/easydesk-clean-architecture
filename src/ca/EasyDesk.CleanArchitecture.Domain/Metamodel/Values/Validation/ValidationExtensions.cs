using FluentValidation;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;

public static partial class ValidationExtensions
{
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

        public IRuleBuilder<T, P> For<V>() where V : AbstractValue<P, V>, IValue<P> =>
            IValue<P>.Companion<V>.ValidationRules(_rules);
    }
}
