namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

public interface IAggregateView<T>
{
    Task<Option<T>> AsOption();

    Task<bool> Exists();
}

public static class AggregateViewExtensions
{
    public static Task<Result<T>> OrElseError<T>(this IAggregateView<T> view, Func<Error> error) =>
        view.AsOption().ThenOrElseError(error);
}
