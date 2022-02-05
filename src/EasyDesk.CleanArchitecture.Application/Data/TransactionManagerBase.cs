using EasyDesk.Tools;
using EasyDesk.Tools.Observables;
using EasyDesk.Tools.Options;
using System;
using System.Threading.Tasks;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Data;

public abstract class TransactionManagerBase<TTransaction> : ITransactionManager, IDisposable
    where TTransaction : IDisposable
{
    private readonly SimpleAsyncEvent<Nothing> _beforeCommit = new();
    private readonly SimpleAsyncEvent<Nothing> _afterCommit = new();
    private Option<TTransaction> _transaction = None;
    private bool _ended = false;

    public IAsyncObservable<Nothing> BeforeCommit => _beforeCommit;

    public IAsyncObservable<Nothing> AfterCommit => _afterCommit;

    protected TTransaction RequireTransaction() => _transaction
        .OrElseThrow(() => new InvalidOperationException("Transaction has not been started"));

    private void EnsureTransactionHasNotAlreadyBeenStarted()
    {
        if (_transaction.IsPresent)
        {
            throw new InvalidOperationException("This transaction has already been started");
        }
    }

    private void EnsureTransactionHasNotAlreadyEnded()
    {
        if (_ended)
        {
            throw new InvalidOperationException("This transaction has already ended");
        }
    }

    public async Task Begin()
    {
        EnsureTransactionHasNotAlreadyBeenStarted();
        _transaction = await BeginTransaction();
    }

    public async Task Commit()
    {
        EnsureTransactionHasNotAlreadyEnded();
        var transaction = RequireTransaction();

        await EmitCommitEvent(_beforeCommit, ex => new BeforeCommitException(ex));

        await CommitTransaction(transaction);
        _ended = true;

        await EmitCommitEvent(_afterCommit, ex => new AfterCommitException(ex));
    }

    private async Task EmitCommitEvent(IAsyncEmitter<Nothing> emitter, Func<Exception, Exception> exceptionWrapper)
    {
        try
        {
            await emitter.Emit();
        }
        catch (Exception ex)
        {
            throw exceptionWrapper(ex);
        }
    }

    public async Task Rollback()
    {
        EnsureTransactionHasNotAlreadyEnded();
        var transaction = RequireTransaction();
        await RollbackTransaction(transaction);
        _ended = true;
    }

    protected abstract Task<TTransaction> BeginTransaction();

    protected abstract Task CommitTransaction(TTransaction transaction);

    protected abstract Task RollbackTransaction(TTransaction transaction);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _transaction.IfPresent(t => t.Dispose());
    }
}
