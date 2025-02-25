using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Data;

public interface IUnitOfWorkManager
{
    Task<Result<T>> RunTransactionally<T>(AsyncFunc<Result<T>> action);
}

public static class UnitOfWorkManagerExtensions
{
    public static async Task<Result<T>> RunTransactionally<T>(this IUnitOfWorkManager unitOfWorkManager, AsyncFunc<T> action) =>
        await unitOfWorkManager.RunTransactionally(async () => Success(await action()));

    public static async Task<Result<Nothing>> RunTransactionally(this IUnitOfWorkManager unitOfWorkManager, AsyncAction action) =>
        await unitOfWorkManager.RunTransactionally(() => ReturningNothing(action));

    public static async Task<Result<T>> RunTransactionally<T>(this IUnitOfWorkManager unitOfWorkManager, Func<Result<T>> action) =>
        await unitOfWorkManager.RunTransactionally(() => Task.FromResult(action()));

    public static async Task<Result<Nothing>> RunTransactionally(this IUnitOfWorkManager unitOfWorkManager, Action action) =>
        await unitOfWorkManager.RunTransactionally(() => ReturningNothing(action));
}
