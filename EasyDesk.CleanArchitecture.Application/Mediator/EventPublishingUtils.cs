using EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools;
using MediatR;
using System;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Application.Mediator
{
    public static class EventPublishingUtils
    {
        public static async Task<Response<Nothing>> PublishDomainEvent(this IMediator mediator, IDomainEvent domainEvent) =>
            await PublishEvent(mediator, domainEvent);

        public static async Task<Response<Nothing>> PublishExternalEvent(this IMediator mediator, IExternalEvent externalEvent) =>
            await PublishEvent(mediator, externalEvent);

        public static async Task<Response<Nothing>> PublishEvent(this IMediator mediator, object ev)
        {
            var contextType = typeof(EventContext<>).MakeGenericType(ev.GetType());
            var eventContext = Activator.CreateInstance(contextType, ev);
            await mediator.Publish(eventContext);
            return (eventContext as IEventContext).Error.Match(
                some: e => Failure<Nothing>(e),
                none: () => Ok);
        }
    }
}
