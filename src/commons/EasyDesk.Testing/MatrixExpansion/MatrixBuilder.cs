using EasyDesk.Commons.Collections;

namespace EasyDesk.Testing.MatrixExpansion;

public class MatrixBuilder
{
    private IEnumerable<IEnumerable<object?>> _matrix = new[] { Enumerable.Empty<object>() };

    private MatrixBuilder AddAxis(IEnumerable<object?> axis)
    {
        _matrix = CartesianProduct(axis);
        return this;
    }

    private IEnumerable<IEnumerable<object?>> CartesianProduct(IEnumerable<object?> newAxis) =>
        _matrix.SelectMany(record => newAxis.Select(element => record.Append(element)));

    public MatrixBuilder Axis<T>(IEnumerable<T> items) => AddAxis(items.Cast<object?>().ToList());

    public MatrixBuilder Axis<T>(params T[] items) => Axis(items.AsEnumerable());

    public MatrixBuilder Filter(Func<IEnumerable<object?>, bool> predicate)
    {
        _matrix = _matrix.Where(predicate);
        return this;
    }

    public IEnumerable<object?[]> Build() => _matrix.Select(e => e.ToArray());
}

public static class MatrixBuilderExtensions
{
    public static MatrixBuilder BooleanAxis(this MatrixBuilder matrixBuilder) =>
        matrixBuilder.Axis(true, false);

    public static MatrixBuilder OptionAxis<T>(this MatrixBuilder matrixBuilder, IEnumerable<T> items) =>
        matrixBuilder.Axis(items.Select(Some<T>).Append(None));

    public static MatrixBuilder OptionAxis<T>(this MatrixBuilder matrixBuilder, T item, params T[] otherItems) =>
        matrixBuilder.OptionAxis(otherItems.Prepend(item));

    public static MatrixBuilder Filter(this MatrixBuilder matrixBuilder, Func<object?[], bool> predicate) =>
        matrixBuilder.Filter(new Func<IEnumerable<object?>, bool>(x => predicate(x.ToArray())));
}
