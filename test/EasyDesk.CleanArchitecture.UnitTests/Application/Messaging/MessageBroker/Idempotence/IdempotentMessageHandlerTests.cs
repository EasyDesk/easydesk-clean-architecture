using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Receiver;
using EasyDesk.CleanArchitecture.Application.Messaging.Receiver.Idempotence;
using EasyDesk.CleanArchitecture.UnitTests.Application.Messaging.MessageBroker;
using NSubstitute;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Messaging.MessageBroker.Idempotence;

public class IdempotentMessageHandlerTests
{
    private readonly IdempotentMessageHandler _sut;
    private readonly IMessageHandler _handler;
    private readonly IIdempotenceManager _idempotenceManager;
    private readonly Message _message = MessageBrokerTestingUtils.NewDefaultMessage();

    public IdempotentMessageHandlerTests()
    {
        _handler = Substitute.For<IMessageHandler>();
        _handler.Handle(_message).Returns(MessageHandlerResult.Handled);

        _idempotenceManager = Substitute.For<IIdempotenceManager>();
        _idempotenceManager.HasBeenProcessed(_message).Returns(false);

        _sut = new(_handler, _idempotenceManager);
    }

    [Theory]
    [InlineData(MessageHandlerResult.Handled)]
    [InlineData(MessageHandlerResult.GenericFailure)]
    [InlineData(MessageHandlerResult.NotSupported)]
    [InlineData(MessageHandlerResult.TransientFailure)]
    public async Task Handle_ShouldReturnTheInnerHandlersResult_IfTheMessageHasNotAlreadyBeenProcessed(
        MessageHandlerResult expectedResult)
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

        result.ShouldBe(MessageHandlerResult.Handled);
    }

    [Fact]
    public async Task Handle_ShouldMarkTheMessageAsProcessed_IfHasNotBeenProcessedBefore()
    {
        await _sut.Handle(_message);

        await _idempotenceManager.Received(1).MarkAsProcessed(_message);
    }
}
