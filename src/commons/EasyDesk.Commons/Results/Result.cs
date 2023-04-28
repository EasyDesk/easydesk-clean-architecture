namespace EasyDesk.Commons;

public readonly record struct Result<T>
{
    private readonly T _value;
    private readonly Error _error;

    public Result(T value)
    {
        _value = value;
        _error = default!;
        IsFailure = false;
    }

    public Result(Error error)
    {
        _value = default!;
        _error = error;
        IsFailure = true;
    }

    public bool IsSuccess => !IsFailure;

    public bool IsFailure { get; }

    public Option<T> Value => Match(
        success: t => Some(t),
        failure: _ => None);

    public Option<Error> Error => Match(
        success: _ => None,
        failure: e => Some(e));

    public T ReadValue() => Match(
        success: t => t,
        failure: _ => throw new InvalidOperationException("Cannot read value from an error result"));

    public Error ReadError() => Match(
        success: _ => throw new InvalidOperationException("Cannot read error from a successful result"),
        failure: e => e);

    public R Match<R>(Func<T, R> success, Func<Error, R> failure) => IsFailure ? failure(_error) : success(_value);

    public void Match(Action<T>? success = null, Action<Error>? failure = null)
    {
        Match(
            success: t => ReturningNothing(() => success?.Invoke(t)),
            failure: e => ReturningNothing(() => failure?.Invoke(e)));
    }

    public Task<R> MatchAsync<R>(AsyncFunc<T, R> success, AsyncFunc<Error, R> failure) =>
        Match(
            success: a => success(a),
            failure: e => failure(e));

    public Task MatchAsync(AsyncAction<T>? success = null, AsyncAction<Error>? failure = null) =>
        Match(
            success: a => success is null ? Task.CompletedTask : success(a),
            failure: e => failure is null ? Task.CompletedTask : failure(e));

    public override string ToString() => Match(
        success: v => $"Success({v})",
        failure: e => $"Failure({e})");

    public static implicit operator Result<Nothing>(Result<T> result) => result.Map(_ => Nothing.Value);

    public static implicit operator Result<T>(T data) => Success(data);

    public static implicit operator Result<T>(Error error) => Failure<T>(error);

    public static Result<T> operator |(Result<T> a, Result<T> b) => a.Match(
        success: _ => a,
        failure: _ => b);

    public static Result<T> operator &(Result<T> a, Result<T> b) => a.Match(
        success: _ => b,
        failure: _ => a);

    public static bool operator true(Result<T> a) => a.IsSuccess;

    public static bool operator false(Result<T> a) => a.IsFailure;
}
