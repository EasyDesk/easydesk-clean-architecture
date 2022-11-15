using EasyDesk.Tools.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class QueryFiltersBuilder
{
    private readonly Dictionary<Type, List<LambdaExpression>> _filters = new();

    public QueryFiltersBuilder AddFilter<T>(Expression<Func<T, bool>> filter)
    {
        _filters.GetOrAdd(typeof(T), () => new()).Add(filter);
        return this;
    }

    internal void ApplyToModelBuilder(ModelBuilder modelBuilder)
    {
        foreach (var (type, filters) in _filters)
        {
        }
    }

    private LambdaExpression CombineQueryFilters(
        Type entityType,
        List<LambdaExpression> filters)
    {
        if (filters.Count == 1)
        {
            return filters[0];
        }

        var newParam = Expression.Parameter(entityType);

        var exp = filters
            .Select(f => ReplacingExpressionVisitor.Replace(f.Parameters.Single(), newParam, f.Body))
            .Aggregate(Expression.AndAlso);

        return Expression.Lambda(exp, newParam);
    }
}
