using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Testing;
using MediatR;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.DomainServices;

public class TransactionalDomainEventQueueTests
{
    private record Event(int Value) : DomainEvent;

    private static readonly Event _event1 = new(1);
    private static readonly Event _event2 = new(2);
    private static readonly Event _event3 = new(3);

    private readonly DomainEventQueue _sut;
    private readonly IMediator _mediator;

    public TransactionalDomainEventQueueTests()
    {
        _mediator = Substitute.For<IMediator>();

        _sut = new(_mediator);
    }

    [Fact]
    public async Task Notify_ShouldEnqueueEventsUntilTheQueueIsFlushed()
    {
        _sut.Notify(_event1);
        _sut.Notify(_event2);
        await _sut.Flush();

        Received.InOrder(() =>
        {
            _mediator.Publish(WithEvent(_event1));
            _mediator.Publish(WithEvent(_event2));
        });
    }

    [Fact]
    public async Task Notify_ShouldStopPublishingEventsToMediator_AsSoonAsOneEventHandlingFails()
    {
        await FailOnPublishOfEvent(_event2);

        _sut.Notify(_event1);
        _sut.Notify(_event2);
        _sut.Notify(_event3);
        await _sut.Flush();

        await _mediator.DidNotReceive().Publish(WithEvent(_event3));
    }

    private async Task FailOnPublishOfEvent(Event ev)
    {
        await _mediator.Publish(Arg.Do<object>(ctx =>
        {
            var eventContext = ctx as EventContext<Event>;
            if (eventContext.EventData == ev)
            {
                eventContext.SetError(TestError.Create());
            }
        }));
    }

    private object WithEvent(Event ev) => Arg.Is<object>(ctx => (ctx as EventContext<Event>).EventData == ev);
}
