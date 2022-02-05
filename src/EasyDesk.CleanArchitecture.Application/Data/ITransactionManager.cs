using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using EasyDesk.Tools.Observables;
using System;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Application.Data;

public interface ITransactionManager
{
    IAsyncObservable<Nothing> BeforeCommit { get; }

    IAsyncObservable<Nothing> AfterCommit { get; }

    Task Begin();

    Task Commit();

    Task Rollback();
}

public static class TransactionManagerExtensions
{
    public static async Task<Response<T>> RunTransactionally<T>(this ITransactionManager transactionManager, AsyncFunc<Response<T>> action)
    {
        await transactionManager.Begin();
        try
        {
            var response = await action();
            await response.MatchAsync(
                success: _ => transactionManager.Commit(),
                failure: _ => transactionManager.Rollback());
            return response;
        }
        catch (AfterCommitException)
        {
            throw;
        }
        catch (Exception)
        {
            await transactionManager.Rollback();
            throw;
        }
    }

    public static async Task<Response<T>> RunTransactionally<T>(this ITransactionManager transactionManager, AsyncFunc<T> action) =>
        await transactionManager.RunTransactionally(async () => Success(await action()));

    public static async Task<Response<Nothing>> RunTransactionally(this ITransactionManager transactionManager, AsyncAction action) =>
        await transactionManager.RunTransactionally(() => Functions.Execute(action));
}
