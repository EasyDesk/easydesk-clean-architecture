using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using EasyDesk.Tools.Observables;
using EasyDesk.Tools.Options;
using System;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Data
{
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
        
        public async Task<Response<Nothing>> Commit()
        {
            EnsureTransactionWasNotAlreadyCommitted();
            _wasCommitted = true;

            var beforeCommitContext = new BeforeCommitContext();
            await _beforeCommit.Emit(beforeCommitContext);

            var commitResult = await CommitIfNotCanceled(beforeCommitContext);

            var afterCommitContext = new AfterCommitContext(commitResult.Error);
            await _afterCommit.Emit(afterCommitContext);

            return commitResult;
        }

        private void EnsureTransactionWasNotAlreadyCommitted()
        {
            if (_wasCommitted)
            {
                throw new InvalidOperationException("This unit of work was already committed");
            }
        }

        private async Task<Response<Nothing>> CommitIfNotCanceled(BeforeCommitContext beforeCommitContext)
        {
            return await beforeCommitContext.Error.Match(
                some: e => Task.FromResult(Failure<Nothing>(e)),
                none: () => CommitTransaction(RequireTransaction()));
        }

        protected abstract Task<Response<Nothing>> CommitTransaction(TTransaction transaction);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _transaction.IfPresent(t => t.Dispose());
        }
    }
}
