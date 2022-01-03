using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Idempotence;
using NSubstitute;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Events.EventBus.Idempotence;

public class IdempotentMessageHandlerTests
{
    private readonly IdempotentMessageHandler _sut;
    private readonly IEventBusMessageHandler _handler;
    private readonly IIdempotenceManager _idempotenceManager;
    private readonly EventBusMessage _message = EventBusTestingUtils.NewDefaultMessage();

    public IdempotentMessageHandlerTests()
    {
        _handler = Substitute.For<IEventBusMessageHandler>();
        _handler.Handle(_message).Returns(EventBusMessageHandlerResult.Handled);

        _idempotenceManager = Substitute.For<IIdempotenceManager>();
        _idempotenceManager.HasBeenProcessed(_message).Returns(false);

        _sut = new(_handler, _idempotenceManager);
    }

    [Theory]
    [InlineData(EventBusMessageHandlerResult.Handled)]
    [InlineData(EventBusMessageHandlerResult.GenericFailure)]
    [InlineData(EventBusMessageHandlerResult.NotSupported)]
    [InlineData(EventBusMessageHandlerResult.TransientFailure)]
    public async Task Handle_ShouldReturnTheInnerHandlersResult_IfTheMessageHasNotAlreadyBeenProcessed(
        EventBusMessageHandlerResult expectedResult)
    {
        _handler.Handle(_message).Returns(expectedResult);

        var actualResult = await _sut.Handle(_message);

        actualResult.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task Handle_ShouldShortCircuit_IfTheMessageHasAlreadyBeenHandled()
    {
        _idempotenceManager.HasBeenProcessed(_message).Returns(true);

        await _sut.Handle(_message);

        await _handler.DidNotReceive().Handle(_message);
    }

    [Fact]
    public async Task Handle_ShouldSucceede_IfTheMessageHasAlreadyBeenHandled()
    {
        _idempotenceManager.HasBeenProcessed(_message).Returns(true);

        var result = await _sut.Handle(_message);

        result.ShouldBe(EventBusMessageHandlerResult.Handled);
    }

    [Fact]
    public async Task Handle_ShouldMarkTheMessageAsProcessed_IfHasNotBeenProcessedBefore()
    {
        await _sut.Handle(_message);

        await _idempotenceManager.Received(1).MarkAsProcessed(_message);
    }
}
