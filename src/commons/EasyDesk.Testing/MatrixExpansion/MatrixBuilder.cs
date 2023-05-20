using System.Collections;

namespace EasyDesk.Testing.MatrixExpansion;

public delegate IEnumerable Expansion(IEnumerable<object> currentParams);

public class MatrixBuilder
{
    private readonly Stack<Expansion> _expansions = new();

    private MatrixBuilder Axis(Expansion expansion)
    {
        _expansions.Push(expansion);
        return this;
    }

    public MatrixBuilder Axis<T>(IEnumerable<T> items) => Axis(_ => items);

    public MatrixBuilder Axis<T>(params T[] items) => Axis(items.AsEnumerable());

    public MatrixBuilder GenerateAxis<T>(Func<IEnumerable<object>, IEnumerable<T>> generator) =>
        Axis(x => generator(x));

    public MatrixBuilder Filter(Func<IEnumerable<object>, bool> predicate)
    {
        var head = _expansions.Pop();
        _expansions.Push(ps => FilteredExpansion(ps, head, predicate));
        return this;
    }

    private IEnumerable<object> FilteredExpansion(IEnumerable<object> currentParams, Expansion expansion, Func<IEnumerable<object>, bool> predicate)
    {
        foreach (var p in expansion(currentParams))
        {
            if (predicate(currentParams.Append(p)))
            {
                yield return p;
            }
        }
    }

    public IEnumerable<object[]> Build()
    {
        var result = new List<object[]>();
        var stack = new List<object>();

        BuildResult(_expansions.ToArray(), stack, result);

        return result;
    }

    private void BuildResult(Expansion[] expansions, List<object> currentParams, List<object[]> result)
    {
        if (currentParams.Count == expansions.Length)
        {
            result.Add(currentParams.ToArray());
            return;
        }
        var expansionIndex = expansions.Length - currentParams.Count - 1;
        var expansion = expansions[expansionIndex];
        var nextParams = expansion(currentParams);
        foreach (var param in nextParams)
        {
            currentParams.Add(param);
            BuildResult(expansions, currentParams, result);
            currentParams.RemoveAt(currentParams.Count - 1);
        }
    }
}

public static class MatrixBuilderExtensions
{
    public static MatrixBuilder BooleanAxis(this MatrixBuilder matrixBuilder) =>
        matrixBuilder.Axis(true, false);

    public static MatrixBuilder OptionAxis<T>(this MatrixBuilder matrixBuilder, IEnumerable<T> items) =>
        matrixBuilder.Axis(items.Select(Some<T>).Append(None));

    public static MatrixBuilder OptionAxis<T>(this MatrixBuilder matrixBuilder, T item, params T[] otherItems) =>
        matrixBuilder.OptionAxis(otherItems.Prepend(item));

    public static MatrixBuilder ResultAxis(this MatrixBuilder matrixBuilder, IEnumerable<Error> errors) =>
        matrixBuilder.Axis(errors.Select(Failure<Nothing>).Prepend(Ok));

    public static MatrixBuilder ResultAxis(this MatrixBuilder matrixBuilder, Error error, params Error[] otherErrors) =>
        matrixBuilder.ResultAxis(otherErrors.Prepend(error));
}
