using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Testing;
using EasyDesk.Tools;
using EasyDesk.Tools.Observables;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Data;

public class TransactionManagerBaseTests
{
    public interface ITestTransaction : IDisposable
    {
        Task Begin();

        Task Commit();

        Task Rollback();
    }

    public class TestTransactionManager : TransactionManagerBase<ITestTransaction>
    {
        private readonly ITestTransaction _testTransaction;

        public TestTransactionManager(ITestTransaction testTransaction)
        {
            _testTransaction = testTransaction;
        }

        protected override async Task<ITestTransaction> BeginTransaction()
        {
            await _testTransaction.Begin();
            return _testTransaction;
        }

        protected override Task CommitTransaction(ITestTransaction transaction) => transaction.Commit();

        protected override Task RollbackTransaction(ITestTransaction transaction) => transaction.Rollback();
    }

    private readonly TestTransactionManager _sut;
    private readonly ITestTransaction _transaction;

    public TransactionManagerBaseTests()
    {
        _transaction = Substitute.For<ITestTransaction>();
        _sut = new(_transaction);
    }

    [Fact]
    public async Task Begin_ShouldStartThePhysicalTransaction()
    {
        await _sut.Begin();

        await _transaction.Received(1).Begin();
    }

    [Fact]
    public async Task Begin_ShouldFail_IfCalledMultipleTimes()
    {
        await _sut.Begin();

        await Should.ThrowAsync<InvalidOperationException>(() => _sut.Begin());
    }

    [Fact]
    public async Task Commit_ShouldCommitThePhysicalTransaction()
    {
        await _sut.Begin();
        await _sut.Commit();

        await _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Commit_ShouldNotifyBeforeCommitHandlers()
    {
        var handler = Substitute.For<AsyncAction<BeforeCommitContext>>();

        _sut.BeforeCommit.Subscribe(handler);
        await _sut.Begin();
        await _sut.Commit();

        await handler.ReceivedWithAnyArgs(1)(default);
    }

    [Fact]
    public async Task Commit_ShouldNotCommitThePhysicalTransaction_IfAnHandlerRequestsCancellation()
    {
        var error = TestError.Create();

        _sut.BeforeCommit.Subscribe(context => context.CancelCommit(error));
        await _sut.Begin();
        await _sut.Commit();

        await _transaction.DidNotReceive().Commit();
    }

    [Fact]
    public async Task Commit_ShouldNotifyAfterCommitHandlers_IfCommitIsSuccessful()
    {
        var handler = Substitute.For<AsyncAction<AfterCommitContext>>();

        _sut.AfterCommit.Subscribe(handler);
        await _sut.Begin();
        await _sut.Commit();

        await handler.Received(1)(Arg.Is<AfterCommitContext>(ctx => ctx.Error.IsAbsent));
    }

    [Fact]
    public async Task Commit_ShouldNotNotifyAfterCommitHandlers_IfCommitFails()
    {
        var handler = Substitute.For<AsyncAction<AfterCommitContext>>();
        _transaction.Commit().Throws<Exception>();

        try
        {
            _sut.AfterCommit.Subscribe(handler);
            await _sut.Begin();
            await _sut.Commit();
        }
        catch
        {
        }

        await handler.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public async Task Commit_ShouldFail_IfTransactionWasNotStarted()
    {
        await Should.ThrowAsync<InvalidOperationException>(() => _sut.Commit());
    }

    [Fact]
    public async Task Commit_ShouldFail_IfCalledMultipleTimes()
    {
        await _sut.Begin();
        await _sut.Commit();

        await Should.ThrowAsync<InvalidOperationException>(() => _sut.Commit());
    }

    [Fact]
    public async Task Dispose_ShouldDisposeTheTransaction()
    {
        await _sut.Begin();
        _sut.Dispose();

        _transaction.Received(1).Dispose();
    }
}
