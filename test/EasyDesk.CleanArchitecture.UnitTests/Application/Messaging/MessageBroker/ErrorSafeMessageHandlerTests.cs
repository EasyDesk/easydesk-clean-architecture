using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Receiver;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Messaging.MessageBroker;

public class ErrorSafeMessageHandlerTests
{
    private readonly ErrorSafeMessageHandler _sut;
    private readonly IMessageHandler _handler;
    private readonly ILogger<ErrorSafeMessageHandler> _logger;
    private readonly Message _message = MessageBrokerTestingUtils.NewDefaultMessage();

    public ErrorSafeMessageHandlerTests()
    {
        _handler = Substitute.For<IMessageHandler>();
        _handler.Handle(_message).Returns(MessageHandlerResult.Handled);

        _logger = Substitute.For<ILogger<ErrorSafeMessageHandler>>();

        _sut = new(_handler, _logger);
    }

    [Theory]
    [InlineData(MessageHandlerResult.Handled)]
    [InlineData(MessageHandlerResult.GenericFailure)]
    [InlineData(MessageHandlerResult.NotSupported)]
    [InlineData(MessageHandlerResult.TransientFailure)]
    public async Task Handle_ShouldReturnTheResultOfTheInnerHandler_IfNoExceptionIsThrown(
        MessageHandlerResult expectedResult)
    {
        _handler.Handle(_message).Returns(expectedResult);

        var actualResult = await _sut.Handle(_message);

        actualResult.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task Handle_ShouldReturnGenericFailure_IfAnExceptionIsThrown()
    {
        _handler.Handle(_message).Throws<Exception>();

        var result = await _sut.Handle(_message);

        result.ShouldBe(MessageHandlerResult.GenericFailure);
    }

    [Fact]
    public async Task Handle_ShouldLogAnErrorMessage_IfAnExceptionIsThrown()
    {
        _handler.Handle(_message).Throws<Exception>();

        await _sut.Handle(_message);

        _logger.ReceivedWithAnyArgs(1).LogError(default(Exception), " ");
    }
}
