using EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Application.Mediator
{
    public abstract class DomainEventPropagator<T> : DomainEventHandlerBase<T>
        where T : DomainEvent
    {
        private readonly IExternalEventPublisher _publisher;

        public DomainEventPropagator(IExternalEventPublisher publisher)
        {
            _publisher = publisher;
        }

        protected override async Task<Response<Nothing>> Handle(T ev)
        {
            await _publisher.Publish(ConvertToExternalEvent(ev));
            return Ok;
        }

        protected abstract ExternalEvent ConvertToExternalEvent(T ev);
    }
}
