using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Testing;
using NSubstitute;
using Shouldly;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Events.EventBus;

public class TransactionalEventBusMessageHandlerTests
{
    private readonly TransactionalEventBusMessageHandler _sut;
    private readonly IEventBusMessageHandler _handler;
    private readonly ITransactionManager _transactionManager;
    private readonly EventBusMessage _message = EventBusTestingUtils.NewDefaultMessage();

    public TransactionalEventBusMessageHandlerTests()
    {
        _handler = Substitute.For<IEventBusMessageHandler>();
        _handler.Handle(_message).Returns(EventBusMessageHandlerResult.Handled);

        _transactionManager = Substitute.For<ITransactionManager>();
        _transactionManager.Commit().Returns(Ok);

        _sut = new(_handler, _transactionManager);
    }

    [Theory]
    [InlineData(EventBusMessageHandlerResult.Handled)]
    [InlineData(EventBusMessageHandlerResult.GenericFailure)]
    [InlineData(EventBusMessageHandlerResult.NotSupported)]
    [InlineData(EventBusMessageHandlerResult.TransientFailure)]
    public async Task Handle_ShouldReturnTheInnerHandlersResult_IfTheCommitSucceedes(
        EventBusMessageHandlerResult expectedResult)
    {
        _handler.Handle(_message).Returns(expectedResult);

        var actualResult = await _sut.Handle(_message);

        actualResult.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task Handle_ShouldWrapTheInnerHandlerInTheUnitOfWork()
    {
        await _sut.Handle(_message);

        Received.InOrder(() =>
        {
            _transactionManager.Begin();
            _handler.Handle(_message);
            _transactionManager.Commit();
        });
    }

    [Fact]
    public async Task Handle_ShouldReturnTransientFailure_IfTheCommitFails()
    {
        _transactionManager.Commit().Returns(TestError.Create());

        var result = await _sut.Handle(_message);

        result.ShouldBe(EventBusMessageHandlerResult.TransientFailure);
    }
}
