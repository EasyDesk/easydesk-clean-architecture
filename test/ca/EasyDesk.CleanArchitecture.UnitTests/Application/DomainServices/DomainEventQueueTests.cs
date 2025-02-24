using Autofac;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Testing.Unit.Domain;
using NSubstitute;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.DomainServices;

public class DomainEventQueueTests
{
    public record Event(int Value) : DomainEvent;

    private static readonly Event _event1 = new(1);
    private static readonly Event _event2 = new(2);
    private static readonly Event _event3 = new(3);

    private readonly DomainEventQueue _sut;
    private readonly DomainEventPublisher _publisher;
    private readonly ISaveChangesHandler _saveChangesHandler;
    private readonly IDomainEventHandler<Event> _handler;

    public DomainEventQueueTests()
    {
        _handler = Substitute.For<IDomainEventHandler<Event>>();

        var builder = new ContainerBuilder();

        builder.RegisterInstance(_handler).SingleInstance();

        _publisher = new(builder.Build());
        _handler.Handle(default!).ReturnsForAnyArgs(Ok);

        _saveChangesHandler = Substitute.For<ISaveChangesHandler>();

        _sut = new(_publisher, _saveChangesHandler);
    }

    [Fact]
    public async Task ShouldEnqueueEventsUntilTheQueueIsFlushed()
    {
        _sut.Notify(_event1);
        _sut.Notify(_event2);
        await _sut.Flush();

        Received.InOrder(() =>
        {
            _handler.Handle(_event1);
            _handler.Handle(_event2);
        });
    }

    [Fact]
    public async Task ShouldStopPublishingEventsAsSoonAsOneHandlerFails()
    {
        _handler.Handle(_event2).Returns(TestDomainError.Create());

        _sut.Notify(_event1);
        _sut.Notify(_event2);
        _sut.Notify(_event3);
        await _sut.Flush();

        await _handler.DidNotReceive().Handle(_event3);
    }
}
