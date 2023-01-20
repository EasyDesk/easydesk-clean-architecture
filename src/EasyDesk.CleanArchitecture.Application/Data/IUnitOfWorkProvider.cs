namespace EasyDesk.CleanArchitecture.Application.Data;

public interface IUnitOfWorkProvider
{
    Option<IUnitOfWork> CurrentUnitOfWork { get; }

    Task<IUnitOfWork> BeginUnitOfWork();

    void EndUnitOfWork();
}

public static class UnitOfWorkProviderExtensions
{
    public static async Task<Result<T>> RunTransactionally<T>(this IUnitOfWorkProvider unitOfWorkProvider, AsyncFunc<Result<T>> action)
    {
        using var unitOfWork = await unitOfWorkProvider.BeginUnitOfWork();
        try
        {
            var result = await action();
            await result.MatchAsync(
                success: _ => unitOfWork.Commit(),
                failure: _ => unitOfWork.Rollback());
            unitOfWorkProvider.EndUnitOfWork();
            return result;
        }
        catch (AfterCommitException)
        {
            throw;
        }
        catch (Exception)
        {
            await unitOfWork.Rollback();
            throw;
        }
    }

    public static async Task<Result<T>> RunTransactionally<T>(this IUnitOfWorkProvider unitOfWorkProvider, AsyncFunc<T> action) =>
        await unitOfWorkProvider.RunTransactionally(async () => Success(await action()));

    public static async Task<Result<Nothing>> RunTransactionally(this IUnitOfWorkProvider unitOfWorkProvider, AsyncAction action) =>
        await unitOfWorkProvider.RunTransactionally(() => ReturningNothing(action));
}
