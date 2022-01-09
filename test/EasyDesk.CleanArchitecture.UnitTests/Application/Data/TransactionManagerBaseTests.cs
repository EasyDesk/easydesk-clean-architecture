using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Testing;
using EasyDesk.Tools;
using EasyDesk.Tools.Observables;
using NSubstitute;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Data;

public class TransactionManagerBaseTests
{
    public interface ITestTransaction : IDisposable
    {
        void Begin();

        Response<Nothing> Commit();
    }

    public class TestUnitOfWork : TransactionManagerBase<ITestTransaction>
    {
        private readonly ITestTransaction _testTransaction;

        public TestUnitOfWork(ITestTransaction testTransaction)
        {
            _testTransaction = testTransaction;
        }

        protected override Task<ITestTransaction> BeginTransaction()
        {
            _testTransaction.Begin();
            return Task.FromResult(_testTransaction);
        }

        protected override Task<Response<Nothing>> CommitTransaction(ITestTransaction transaction) =>
            Task.FromResult(transaction.Commit());
    }

    private readonly TestUnitOfWork _sut;
    private readonly ITestTransaction _transaction;

    public TransactionManagerBaseTests()
    {
        _transaction = Substitute.For<ITestTransaction>();
        _transaction.Commit().Returns(Ok);
        _sut = new(_transaction);
    }

    [Fact]
    public async Task Begin_ShouldStartThePhysicalTransaction()
    {
        await _sut.Begin();

        _transaction.Received(1).Begin();
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

        _transaction.Received(1).Commit();
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
        var commitResult = await _sut.Commit();

        commitResult.ShouldBe(Failure<Nothing>(error));
        _transaction.DidNotReceive().Commit();
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
    public async Task Commit_ShouldNotifyAfterCommitHandlers_IfCommitFails()
    {
        var handler = Substitute.For<AsyncAction<AfterCommitContext>>();
        var error = Errors.Generic("Commit failed");
        _transaction.Commit().Returns(Failure<Nothing>(error));

        _sut.AfterCommit.Subscribe(handler);
        await _sut.Begin();
        await _sut.Commit();

        await handler.Received(1)(Arg.Is<AfterCommitContext>(ctx => ctx.Error.Contains(error)));
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
