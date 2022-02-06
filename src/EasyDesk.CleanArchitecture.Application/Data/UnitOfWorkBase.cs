using EasyDesk.Tools;
using EasyDesk.Tools.Observables;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Data;

public abstract class UnitOfWorkBase<T> : IUnitOfWork
    where T : IDisposable
{
    private readonly SimpleAsyncEvent<Nothing> _beforeCommit = new();
    private readonly SimpleAsyncEvent<Nothing> _afterCommit = new();
    private bool _ended = false;

    public UnitOfWorkBase(T transaction)
    {
        Transaction = transaction;
    }

    public IAsyncObservable<Nothing> BeforeCommit => _beforeCommit;

    public IAsyncObservable<Nothing> AfterCommit => _afterCommit;

    protected T Transaction { get; }

    private void EnsureHasNotAlreadyEnded()
    {
        if (_ended)
        {
            throw new InvalidOperationException("This unit of work has already ended");
        }
    }

    public async Task Commit()
    {
        EnsureHasNotAlreadyEnded();

        await EmitCommitEvent(_beforeCommit, ex => new BeforeCommitException(ex));

        await CommitTransaction();
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
        EnsureHasNotAlreadyEnded();
        await RollbackTransaction();
        _ended = true;
    }

    protected abstract Task CommitTransaction();

    protected abstract Task RollbackTransaction();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Transaction.Dispose();
    }
}
