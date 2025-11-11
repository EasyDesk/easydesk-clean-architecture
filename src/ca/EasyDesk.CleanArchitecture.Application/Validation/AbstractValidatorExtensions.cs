using EasyDesk.Commons.Options;
using FluentValidation;
using FluentValidation.Internal;
using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public static class AbstractValidatorExtensions
{
    public static void RuleForOption<T, TProperty>(this AbstractValidator<T> validator, Expression<Func<T, Option<TProperty>>> property, Func<IRuleBuilderOptions<T, TProperty>, IRuleBuilder<T, TProperty>> rules)
    {
        var propertySelector = property.Compile();
        validator.When(x => propertySelector(x).IsPresent, () =>
        {
            rules(validator.RuleFor(x => propertySelector(x).Value)
                .SetValidator(new InlineValidator<TProperty>())
                .OverridePropertyName(property.GetMember()?.Name ?? $"{typeof(TProperty).Name}"));
        });
    }
}
