namespace EasyDesk.Commons;

public static partial class StaticImports
{
    public static Result<Nothing> Ok { get; } = Success(Nothing.Value);

    public static Result<T> Success<T>(T data) where T : notnull => new(data);

    public static Result<T> Failure<T>(Error error) where T : notnull => new(error);

    public static Result<Nothing> Ensure(bool condition, Func<Error> otherwise) =>
        condition ? Ok : Failure<Nothing>(otherwise());

    public static Result<Nothing> EnsureNot(bool condition, Func<Error> otherwise) =>
        Ensure(!condition, otherwise);

    public static Result<T> Ensure<T>(T subject, Func<T, bool> predicate, Func<T, Error> otherwise) where T : notnull =>
        predicate(subject) ? Success(subject) : Failure<T>(otherwise(subject));

    public static Result<T> EnsureNot<T>(T subject, Func<T, bool> predicate, Func<T, Error> otherwise) where T : notnull =>
        Ensure(subject, t => !predicate(t), otherwise);

    public static Result<T> OrElseError<T>(this Option<T> option, Func<Error> error) where T : notnull => option.Match<Result<T>>(
        some: t => t,
        none: () => error());

    public static async Task<Result<T>> ThenOrElseError<T>(this Task<Option<T>> option, Func<Error> error) where T : notnull =>
        (await option).OrElseError(error);
}
