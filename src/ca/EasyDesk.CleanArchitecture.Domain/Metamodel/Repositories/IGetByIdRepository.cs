using EasyDesk.CleanArchitecture.Domain.Model.Errors;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

public interface IGetByIdRepository<T, TId>
    where T : AggregateRoot
{
    Task<Option<T>> GetById(TId id);

    Task<bool> Exists(TId id);
}

public static class GetByIdRepositoryExtensions
{
    public static async Task<Result<T>> RequireById<T, TId>(this IGetByIdRepository<T, TId> repository, TId id)
        where T : AggregateRoot
    {
        return await repository
            .GetById(id)
            .ThenOrElseError(AggregateNotFound.OfType<T>);
    }
}
