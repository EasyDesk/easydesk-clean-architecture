using EasyDesk.Tools;
using EasyDesk.Tools.Observables;
using EasyDesk.Tools.Options;
using System;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Data;

public abstract class TransactionManagerBase<TTransaction> : ITransactionManager
    where TTransaction : IDisposable
{
    private readonly SimpleAsyncEvent<BeforeCommitContext> _beforeCommit = new();
    private readonly SimpleAsyncEvent<AfterCommitContext> _afterCommit = new();
    private bool _wasCommitted = false;
    private Option<TTransaction> _transaction = None;

    public IAsyncObservable<BeforeCommitContext> BeforeCommit => _beforeCommit;

    public IAsyncObservable<AfterCommitContext> AfterCommit => _afterCommit;

    protected TTransaction RequireTransaction() => _transaction
        .OrElseThrow(() => new InvalidOperationException("Unit of work has not been started"));

    public async Task Begin()
    {
        EnsureTransactionHasNotAlreadyBeenStarted();
        var transaction = await BeginTransaction();
        _transaction = Some(transaction);
    }

    private void EnsureTransactionHasNotAlreadyBeenStarted()
    {
        if (_transaction.IsPresent)
        {
            throw new InvalidOperationException("This unit of work has already been started");
        }
    }

    protected abstract Task<TTransaction> BeginTransaction();

    public async Task Commit()
    {
        EnsureTransactionWasNotAlreadyCommitted();
        _wasCommitted = true;

        var beforeCommitContext = new BeforeCommitContext();
        await _beforeCommit.Emit(beforeCommitContext);

        await CommitIfNotCanceled(beforeCommitContext);

        var afterCommitContext = new AfterCommitContext(beforeCommitContext.Error);
        await _afterCommit.Emit(afterCommitContext);
    }

    private void EnsureTransactionWasNotAlreadyCommitted()
    {
        if (_wasCommitted)
        {
            throw new InvalidOperationException("This unit of work was already committed");
        }
    }

    private async Task CommitIfNotCanceled(BeforeCommitContext beforeCommitContext)
    {
        await beforeCommitContext.Error.Match(
            some: e => Task.FromResult(Failure<Nothing>(e)),
            none: () => CommitTransaction(RequireTransaction()));
    }

    protected abstract Task CommitTransaction(TTransaction transaction);

    public async Task Rollback()
    {
        await RollbackTransaction(RequireTransaction());
    }

    protected abstract Task RollbackTransaction(TTransaction transaction);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _transaction.IfPresent(t => t.Dispose());
    }
}
