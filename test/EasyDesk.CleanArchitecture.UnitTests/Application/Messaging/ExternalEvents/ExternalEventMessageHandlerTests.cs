using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Messaging.ExternalEvents;
using EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker;
using EasyDesk.CleanArchitecture.Testing;
using NSubstitute;
using Shouldly;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Messaging.MessageBroker;

public class ExternalEventMessageHandlerTests
{
    private record SupportedEvent : ExternalEvent;

    private const string Json = "{}";

    private readonly ExternalEventMessageHandler _sut;
    private readonly IExternalEventHandler _externalEventHandler;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IEnumerable<Type> _supportedTypes = Items(typeof(SupportedEvent));
    private readonly SupportedEvent _event = new();
    private readonly Message _supportedMessage;

    public ExternalEventMessageHandlerTests()
    {
        _supportedMessage = MessageBrokerTestingUtils.NewMessageWithType(typeof(SupportedEvent).GetEventTypeName());

        _externalEventHandler = Substitute.For<IExternalEventHandler>();
        _externalEventHandler.Handle(_event).Returns(Ok);

        _jsonSerializer = Substitute.For<IJsonSerializer>();
        _jsonSerializer.Deserialize(Json, typeof(SupportedEvent)).Returns(_event);

        _sut = new(_externalEventHandler, _jsonSerializer, _supportedTypes);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_IfTheEventTypeIsSupportedAndTheExternalEventHandlerSucceeds()
    {
        var result = await _sut.Handle(_supportedMessage);

        result.ShouldBe(MessageHandlerResult.Handled);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotSupported_IfTheEventTypeIsNotSupported()
    {
        var result = await _sut.Handle(MessageBrokerTestingUtils.NewMessageWithType("UNSUPPORTED"));

        result.ShouldBe(MessageHandlerResult.NotSupported);
    }

    [Fact]
    public async Task Handle_ShouldReturnGenericFailurte_IfTheExternalEventHandlerFails()
    {
        _externalEventHandler.Handle(_event).Returns(TestError.Create());

        var result = await _sut.Handle(_supportedMessage);

        result.ShouldBe(MessageHandlerResult.GenericFailure);
    }
}
