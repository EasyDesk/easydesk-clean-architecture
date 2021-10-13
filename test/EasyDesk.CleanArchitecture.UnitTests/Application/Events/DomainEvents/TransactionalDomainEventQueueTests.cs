using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Events.DomainEvents;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Testing;
using EasyDesk.Tools.Observables;
using MediatR;
using NSubstitute;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Events.DomainEvents
{
    public class TransactionalDomainEventQueueTests
    {
        private record Event(int Value) : DomainEvent;

        private static readonly Event _event1 = new(1);
        private static readonly Event _event2 = new(2);
        private static readonly Event _event3 = new(3);

        private readonly TransactionalDomainEventQueue _sut;
        private readonly SimpleAsyncEvent<BeforeCommitContext> _beforeCommit;
        private readonly ITransactionManager _transactionManager;
        private readonly IMediator _mediator;

        public TransactionalDomainEventQueueTests()
        {
            _beforeCommit = new();

            _transactionManager = Substitute.For<ITransactionManager>();
            _transactionManager.BeforeCommit.Returns(_beforeCommit);

            _mediator = Substitute.For<IMediator>();

            _sut = new(_transactionManager, _mediator);
        }

        [Fact]
        public async Task Notify_ShouldEnqueueEventsUntilTheUnitOfWorkIsCommitted()
        {
            _sut.Notify(_event1);
            _sut.Notify(_event2);
            await Commit();

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
            await Commit();

            await _mediator.DidNotReceive().Publish(WithEvent(_event3));
        }

        [Fact]
        public async Task Notify_ShouldCancelTheCommit_IfAnyHandlerFails()
        {
            await FailOnPublishOfEvent(_event1);

            _sut.Notify(_event1);
            var context = await Commit();

            context.Error.ShouldNotBeEmpty();
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

        private async Task<BeforeCommitContext> Commit()
        {
            var context = new BeforeCommitContext();
            await _beforeCommit.Emit(context);
            return context;
        }
    }
}
