using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Events.EventBus
{
    public class ErrorSafeEventBusMessageHandlerTests
    {
        private readonly ErrorSafeEventBusMessageHandler _sut;
        private readonly IEventBusMessageHandler _handler;
        private readonly ILogger<ErrorSafeEventBusMessageHandler> _logger;
        private readonly EventBusMessage _message = EventBusTestingUtils.NewDefaultMessage();

        public ErrorSafeEventBusMessageHandlerTests()
        {
            _handler = Substitute.For<IEventBusMessageHandler>();
            _handler.Handle(_message).Returns(EventBusMessageHandlerResult.Handled);

            _logger = Substitute.For<ILogger<ErrorSafeEventBusMessageHandler>>();

            _sut = new(_handler, _logger);
        }

        [Theory]
        [InlineData(EventBusMessageHandlerResult.Handled)]
        [InlineData(EventBusMessageHandlerResult.GenericFailure)]
        [InlineData(EventBusMessageHandlerResult.NotSupported)]
        [InlineData(EventBusMessageHandlerResult.TransientFailure)]
        public async Task Handle_ShouldReturnTheResultOfTheInnerHandler_IfNoExceptionIsThrown(
            EventBusMessageHandlerResult expectedResult)
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

            result.ShouldBe(EventBusMessageHandlerResult.GenericFailure);
        }

        [Fact]
        public async Task Handle_ShouldLogAnErrorMessage_IfAnExceptionIsThrown()
        {
            _handler.Handle(_message).Throws<Exception>();

            await _sut.Handle(_message);

            _logger.ReceivedWithAnyArgs(1).LogError(default(Exception), default);
        }
    }
}
