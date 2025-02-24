using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Data;

public interface IUnitOfWorkManager
{
    Task<Result<T>> RunTransactionally<T>(AsyncFunc<Result<T>> action);
}

public static class UnitOfWorkProviderExtensions
{
    public static async Task<Result<T>> RunTransactionally<T>(this IUnitOfWorkManager unitOfWorkProvider, AsyncFunc<T> action) =>
        await unitOfWorkProvider.RunTransactionally(async () => Success(await action()));

    public static async Task<Result<Nothing>> RunTransactionally(this IUnitOfWorkManager unitOfWorkProvider, AsyncAction action) =>
        await unitOfWorkProvider.RunTransactionally(() => ReturningNothing(action));

    public static async Task<Result<T>> RunTransactionally<T>(this IUnitOfWorkManager unitOfWorkProvider, Func<Result<T>> action) =>
        await unitOfWorkProvider.RunTransactionally(() => Task.FromResult(action()));

    public static async Task<Result<Nothing>> RunTransactionally(this IUnitOfWorkManager unitOfWorkProvider, Action action) =>
        await unitOfWorkProvider.RunTransactionally(() => ReturningNothing(action));
}
