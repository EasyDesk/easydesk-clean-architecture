using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Testing.Unit.Domain;
using NSubstitute;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.DomainServices;

public class DomainEventQueueTests
{
    private record Event(int Value) : DomainEvent;

    private static readonly Event _event1 = new(1);
    private static readonly Event _event2 = new(2);
    private static readonly Event _event3 = new(3);

    private readonly DomainEventQueue _sut;
    private readonly IDomainEventPublisher _publisher;
    private readonly ISaveChangesHandler _saveChangesHandler;

    public DomainEventQueueTests()
    {
        _publisher = Substitute.For<IDomainEventPublisher>();
        _publisher.Publish(default!).ReturnsForAnyArgs(Ok);

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
            _publisher.Publish(_event1);
            _publisher.Publish(_event2);
        });
    }

    [Fact]
    public async Task ShouldStopPublishingEventsAsSoonAsOneHandlerFails()
    {
        _publisher.Publish(_event2).Returns(TestDomainError.Create());

        _sut.Notify(_event1);
        _sut.Notify(_event2);
        _sut.Notify(_event3);
        await _sut.Flush();

        await _publisher.DidNotReceive().Publish(_event3);
    }
}
