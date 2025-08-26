using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Results;
using NSubstitute;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Data;

public class UnitOfWorkBaseTests
{
    public interface ITestTransaction : IDisposable
    {
        Task Commit();

        Task Rollback();
    }

    public class TestUnitOfWorkManager : UnitOfWorkManager<ITestTransaction>
    {
        private readonly Func<ITestTransaction> _factory;

        public TestUnitOfWorkManager(Func<ITestTransaction> factory)
        {
            _factory = factory;
        }

        protected override Task Commit(ITestTransaction transaction) => transaction.Commit();

        protected override Task<ITestTransaction> CreateTransaction() => Task.FromResult(_factory());

        protected override Task Rollback(ITestTransaction transaction) => transaction.Rollback();
    }

    private readonly TestUnitOfWorkManager _sut;
    private readonly ITestTransaction _transaction;

    protected Result<Nothing> Failure => Failure<Nothing>(Errors.NotFound());

    public UnitOfWorkBaseTests()
    {
        _transaction = Substitute.For<ITestTransaction>();
        _sut = new(() => _transaction);
    }

    public static TheoryData<Func<Result<Nothing>>> FailureLambdas()
    {
        return new(
        [
            () => Failure<Nothing>(Errors.NotFound()),
            () => throw new InvalidOperationException(),
        ]);
    }

    public static TheoryData<Result<Nothing>> Results()
    {
        return
        [
            Failure<Nothing>(Errors.NotFound()),
            Ok,
        ];
    }

    [Fact]
    public void ShouldPropagateExceptions()
    {
        Should.Throw<Exception>(() => _sut.RunTransactionally(() => throw new InvalidOperationException()));
    }

    [Theory]
    [MemberData(nameof(Results))]
    public async Task ShouldPropagateResults(Result<Nothing> result)
    {
        (await _sut.RunTransactionally(() => result)).ShouldBe(result);
    }

    [Fact]
    public async Task ShouldCommitThePhysicalTransactionWhenCommitted()
    {
        await _sut.RunTransactionally(() => Ok);

        await _transaction.Received(1).Commit();
    }

    [Theory]
    [MemberData(nameof(FailureLambdas))]
    public async Task ShouldNotCommitThePhysicalTransactionWhenRollbacked(Func<Result<Nothing>> failingLambda)
    {
        try
        {
            await _sut.RunTransactionally(failingLambda);
        }
        catch
        {
        }

        await _transaction.DidNotReceiveWithAnyArgs().Commit();
    }

    [Fact]
    public async Task ShouldNotRollbackThePhysicalTransactionWhenCommitted()
    {
        await _sut.RunTransactionally(() => Ok);

        await _transaction.DidNotReceiveWithAnyArgs().Rollback();
    }

    [Theory]
    [MemberData(nameof(FailureLambdas))]
    public async Task ShouldRollbackThePhysicalTransactionOnFailure(Func<Result<Nothing>> failingLambda)
    {
        try
        {
            await _sut.RunTransactionally(failingLambda);
        }
        catch
        {
        }

        await _transaction.Received(1).Rollback();
    }

    [Fact]
    public async Task ShouldDisposeTheInnerTransactionAfterCommit()
    {
        await _sut.RunTransactionally(() => Ok);

        _transaction.Received(1).Dispose();
    }

    [Theory]
    [MemberData(nameof(FailureLambdas))]
    public async Task ShouldDisposeTheInnerTransactionAfterRollback(Func<Result<Nothing>> failingLambda)
    {
        try
        {
            await _sut.RunTransactionally(failingLambda);
        }
        catch
        {
        }

        _transaction.Received(1).Dispose();
    }
}
