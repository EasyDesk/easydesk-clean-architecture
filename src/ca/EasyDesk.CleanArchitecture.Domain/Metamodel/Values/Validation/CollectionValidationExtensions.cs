using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using FluentValidation;
using FluentValidation.Internal;
using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;

public static class CollectionValidationExtensions
{
    public static IRuleBuilder<T, IEnumerable<TItem>> NotContainingDuplicatesFor<T, TItem, TProperty>(this IRuleBuilder<T, IEnumerable<TItem>> rule, Expression<Func<TItem, TProperty>> property, IEqualityComparer<TProperty>? comparer = null)
    {
        var member = property.GetMember();
        var func = AccessorCache<TItem>.GetCachedAccessor(member, property);
        var propertyName = ValidatorOptions.Global.PropertyNameResolver(typeof(TItem), member, property);
        return rule
            .Must((_, xs, ctx) => FindDuplicate(xs.Select(func), comparer)
                .IfPresent(p => ctx.MessageFormatter.AppendArgument("DuplicateValue", p))
                .IsAbsent)
            .WithErrorCode("NotContainingDuplicatesFor")
            .WithMessage($$"""'{PropertyName}' contains duplicate values for property '{{propertyName}}'.""");
    }

    public static IRuleBuilder<T, IEnumerable<TItem>> NotContainingDuplicates<T, TItem>(this IRuleBuilder<T, IEnumerable<TItem>> rule, IEqualityComparer<TItem>? comparer = null) => rule
        .Must((_, xs, ctx) => FindDuplicate(xs, comparer)
            .IfPresent(p => ctx.MessageFormatter.AppendArgument("DuplicateValue", p))
            .IsAbsent)
        .WithErrorCode("NotContainingDuplicates")
        .WithMessage("'{PropertyName}' contains duplicate values.");

    private static Option<T> FindDuplicate<T>(IEnumerable<T> items, IEqualityComparer<T>? comparer)
    {
        var set = new HashSet<T>(comparer);
        return items.FirstOption(x => !set.Add(x));
    }
}
