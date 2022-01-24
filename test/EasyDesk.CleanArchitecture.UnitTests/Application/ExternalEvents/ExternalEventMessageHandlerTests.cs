using System.Threading.Tasks;
using EasyDesk.CleanArchitecture.Application.Messaging.Receiver;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing;
using NSubstitute;
using Shouldly;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.ExternalEvents;

public class ExternalEventMessageHandlerTests
{
    private record TestEvent : ExternalEvent;

    private readonly ExternalEventMessageHandler _sut;
    private readonly IExternalEventHandler _externalEventHandler;
    private readonly TestEvent _event = new();
    private readonly Message _message;

    public ExternalEventMessageHandlerTests()
    {
        _message = Message.Create(new TestEvent());

        _externalEventHandler = Substitute.For<IExternalEventHandler>();
        _externalEventHandler.Handle(_event).Returns(Ok);

        _sut = new(_externalEventHandler);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_IfTheEventTypeIsSupportedAndTheExternalEventHandlerSucceeds()
    {
        var result = await _sut.Handle(_message);

        result.ShouldBe(MessageHandlerResult.Handled);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotSupported_IfTheMessageContentIsNotAnExternalEvent()
    {
        var result = await _sut.Handle(Message.Create(new object()));

        result.ShouldBe(MessageHandlerResult.NotSupported);
    }

    [Fact]
    public async Task Handle_ShouldReturnGenericFailurte_IfTheExternalEventHandlerFails()
    {
        _externalEventHandler.Handle(_event).Returns(TestError.Create());

        var result = await _sut.Handle(_message);

        result.ShouldBe(MessageHandlerResult.GenericFailure);
    }
}
