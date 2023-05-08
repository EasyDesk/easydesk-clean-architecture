using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public static class Extensions
{
    public static Task<Result<T>> OrElseNotFound<T>(this IAggregateView<T> view) =>
        view.OrElseError(Errors.NotFound);
}
