using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using EasyDesk.Tools.Observables;
using EasyDesk.Tools.Options;
using System;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Data;

public interface ITransactionManager : IDisposable
{
    IAsyncObservable<BeforeCommitContext> BeforeCommit { get; }

    IAsyncObservable<AfterCommitContext> AfterCommit { get; }

    Task Begin();

    Task<Response<Nothing>> Commit();
}

public static class TransactionManagerExtensions
{
    public static async Task<Response<T>> RunTransactionally<T>(this ITransactionManager transactionManager, AsyncFunc<Response<T>> action)
    {
        await transactionManager.Begin();
        return await action()
            .ThenRequireAsync(_ => transactionManager.Commit());
    }

    public static async Task<Response<T>> RunTransactionally<T>(this ITransactionManager transactionManager, AsyncFunc<T> action) =>
        await transactionManager.RunTransactionally(async () => Success(await action()));
}

public class BeforeCommitContext
{
    public Option<Error> Error { get; private set; } = None;

    public void CancelCommit(Error error)
    {
        Error = Some(error);
    }
}

public class AfterCommitContext
{
    public AfterCommitContext(Option<Error> error)
    {
        Error = error;
    }

    public bool Successful => Error.IsAbsent;

    public Option<Error> Error { get; }
}
