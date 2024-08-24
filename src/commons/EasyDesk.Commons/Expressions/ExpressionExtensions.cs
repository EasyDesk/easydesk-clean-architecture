using System.Linq.Expressions;

namespace EasyDesk.Commons.Expressions;

public static class ExpressionExtensions
{
    public static Expression ReplaceParameter(this Expression expression, ParameterExpression toReplace, Expression newExpression) =>
        new ParameterReplaceVisitor(toReplace, newExpression).Visit(expression);

    private class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly Expression _to;

        public ParameterReplaceVisitor(ParameterExpression from, Expression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _from ? _to : node;
        }
    }

    public static Expression<Func<TSource, TResult>> Compose<TSource, TIntermediate, TResult>(
        this Expression<Func<TSource, TIntermediate>> first,
        Expression<Func<TIntermediate, TResult>> second)
    {
        var param = Expression.Parameter(typeof(TSource));
        var intermediateValue = first.Body.ReplaceParameter(first.Parameters[0], param);
        var body = second.Body.ReplaceParameter(second.Parameters[0], intermediateValue);
        return Expression.Lambda<Func<TSource, TResult>>(body, param);
    }
}
