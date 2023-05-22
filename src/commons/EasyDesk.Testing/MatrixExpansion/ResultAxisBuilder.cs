namespace EasyDesk.Testing.MatrixExpansion;

public class ResultAxisBuilder<T>
{
    private readonly List<Result<T>> _axis = new();

    internal ResultAxisBuilder()
    {
    }

    public ResultAxisBuilder<T> Result(Result<T> result)
    {
        _axis.Add(result);
        return this;
    }

    public ResultAxisBuilder<T> Results(IEnumerable<Result<T>> results)
    {
        _axis.AddRange(results);
        return this;
    }

    internal IEnumerable<Result<T>> Build() =>
        _axis;
}

public static class ResultAxisBuilderExtensions
{
    public static MatrixBuilder ResultAxis<T>(this MatrixBuilder matrix, Action<ResultAxisBuilder<T>> builder)
    {
        var b = new ResultAxisBuilder<T>();
        builder(b);
        return matrix.Axis(b.Build());
    }

    public static ResultAxisBuilder<T> Success<T>(this ResultAxisBuilder<T> builder, T result) =>
        builder.Result(result);

    public static ResultAxisBuilder<T> Failure<T>(this ResultAxisBuilder<T> builder, Error error) =>
        builder.Result(error);

    public static ResultAxisBuilder<T> Successes<T>(this ResultAxisBuilder<T> builder, IEnumerable<T> successes) =>
        builder.Results(successes.Select(StaticImports.Success));

    public static ResultAxisBuilder<T> Successes<T>(this ResultAxisBuilder<T> builder, T first, T second, params T[] others) =>
        builder.Successes(others.Prepend(second).Prepend(first));

    public static ResultAxisBuilder<T> Failures<T>(this ResultAxisBuilder<T> builder, IEnumerable<Error> failures) =>
        builder.Results(failures.Select(StaticImports.Failure<T>));

    public static ResultAxisBuilder<T> Failures<T>(this ResultAxisBuilder<T> builder, Error first, Error second, params Error[] others) =>
        builder.Failures(others.Prepend(second).Prepend(first));

    public static ResultAxisBuilder<Nothing> Success(this ResultAxisBuilder<Nothing> builder) => builder.Result(Ok);
}
