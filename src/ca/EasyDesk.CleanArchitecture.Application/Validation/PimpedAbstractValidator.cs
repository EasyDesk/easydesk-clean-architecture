using FluentValidation;
using FluentValidation.Internal;
using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public abstract class PimpedAbstractValidator<T> : AbstractValidator<T>
{
    public void RuleForOption<TProperty>(Expression<Func<T, Option<TProperty>>> property, Func<IRuleBuilderOptions<T, TProperty>, IRuleBuilderOptions<T, TProperty>> rules)
    {
        var propertySelector = property.Compile();
        When(x => propertySelector(x).IsPresent, () =>
        {
            rules(RuleFor(x => propertySelector(x).Value)
                .SetValidator(new InlineValidator<TProperty>())
                .OverridePropertyName(property.GetMember()?.Name ?? $"{typeof(TProperty).Name}"));
        });
    }

    public void RuleForOption<TProperty>(Expression<Func<T, Option<TProperty>>> property, IValidator<TProperty> validator) =>
        RuleForOption(property, rules => rules.SetValidator(validator));

    public void RuleForEachOption<TProperty>(Expression<Func<T, IEnumerable<Option<TProperty>>>> properties, Func<IRuleBuilderOptions<T, TProperty>, IRuleBuilderOptions<T, TProperty>> rules)
    {
        var propertiesSelector = properties.Compile();
        rules(RuleForEach(x => propertiesSelector(x).SelectMany(o => o))
            .SetValidator(new InlineValidator<TProperty>())
            .OverridePropertyName(properties.GetMember()?.Name ?? $"Every {typeof(TProperty).Name}"));
    }
}
