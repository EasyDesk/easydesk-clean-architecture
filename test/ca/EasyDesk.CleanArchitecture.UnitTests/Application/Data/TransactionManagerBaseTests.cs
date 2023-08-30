using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.Commons.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Data;

public class UnitOfWorkBaseTests
{
    public interface ITestTransaction : IDisposable
    {
        Task Commit();

        Task Rollback();
    }

    public class TestUnitOfWork : UnitOfWorkBase<ITestTransaction>
    {
        public TestUnitOfWork(ITestTransaction testTransaction) : base(testTransaction)
        {
        }

        protected override Task CommitTransaction() => Transaction.Commit();

        protected override Task RollbackTransaction() => Transaction.Rollback();
    }

    private readonly TestUnitOfWork _sut;
    private readonly ITestTransaction _transaction;

    public UnitOfWorkBaseTests()
    {
        _transaction = Substitute.For<ITestTransaction>();
        _sut = new(_transaction);
    }

    [Fact]
    public async Task ShouldCommitThePhysicalTransactionWhenCommitted()
    {
        await _sut.Commit();

        await _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task ShouldNotifyBeforeCommitHandlersWhenCommitted()
    {
        var handler = Substitute.For<AsyncAction<Nothing>>();

        _sut.BeforeCommit.Subscribe(handler);
        await _sut.Commit();

        await handler.ReceivedWithAnyArgs(1)(default);
    }

    [Fact]
    public async Task ShouldNotifyAfterCommitHandlersWhenCommitted_IfCommitIsSuccessful()
    {
        var handler = Substitute.For<AsyncAction<Nothing>>();

        _sut.AfterCommit.Subscribe(handler);
        await _sut.Commit();

        await handler.Received(1)(Nothing.Value);
    }

    [Fact]
    public async Task ShouldNotNotifyAfterCommitHandlers_IfCommitFails()
    {
        var handler = Substitute.For<AsyncAction<Nothing>>();
        _transaction.Commit().Throws<Exception>();

        _sut.AfterCommit.Subscribe(handler);
        await Should.ThrowAsync<Exception>(_sut.Commit);

        await handler.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public async Task ShouldFailCommittingMultipleTimes()
    {
        await _sut.Commit();

        await Should.ThrowAsync<InvalidOperationException>(_sut.Commit);
    }

    [Fact]
    public async Task ShouldFailCommittingAfterRollback()
    {
        await _sut.Rollback();

        await Should.ThrowAsync<InvalidOperationException>(_sut.Commit);
    }

    [Fact]
    public void ShouldDisposeTheInnerTransactionWhenDisposed()
    {
        _sut.Dispose();

        _transaction.Received(1).Dispose();
    }
}
