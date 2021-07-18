using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.Testing.Utils;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Events.EventBus
{
    public class DefaultEventBusMessageHandlerTests
    {
        private record SupportedEvent : IExternalEvent;

        private const string _json = "{}";

        private readonly DefaultEventBusMessageHandler _sut;
        private readonly IExternalEventHandler _externalEventHandler;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IEnumerable<Type> _supportedTypes = Items(typeof(SupportedEvent));
        private readonly SupportedEvent _event = new();
        private readonly EventBusMessage _supportedMessage;

        public DefaultEventBusMessageHandlerTests()
        {
            _supportedMessage = EventBusTestingUtils.NewMessageWithType(typeof(SupportedEvent).GetEventTypeName());

            _externalEventHandler = Substitute.For<IExternalEventHandler>();
            _externalEventHandler.Handle(_event).Returns(Ok);

            _jsonSerializer = Substitute.For<IJsonSerializer>();
            _jsonSerializer.Deserialize(_json, typeof(SupportedEvent)).Returns(_event);

            _sut = new(_externalEventHandler, _jsonSerializer, _supportedTypes);
        }

        [Fact]
        public async Task Handle_ShouldSucceed_IfTheEventTypeIsSupportedAndTheExternalEventHandlerSucceeds()
        {
            var result = await _sut.Handle(_supportedMessage);

            result.ShouldBe(EventBusMessageHandlerResult.Handled);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotSupported_IfTheEventTypeIsNotSupported()
        {
            var result = await _sut.Handle(EventBusTestingUtils.NewMessageWithType("UNSUPPORTED"));

            result.ShouldBe(EventBusMessageHandlerResult.NotSupported);
        }

        [Fact]
        public async Task Handle_ShouldReturnGenericFailurte_IfTheExternalEventHandlerFails()
        {
            _externalEventHandler.Handle(_event).Returns(TestError.Create());

            var result = await _sut.Handle(_supportedMessage);

            result.ShouldBe(EventBusMessageHandlerResult.GenericFailure);
        }
    }
}
