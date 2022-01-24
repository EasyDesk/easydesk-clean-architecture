using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Sender;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Application.Mediator;

public abstract class DomainEventPropagator<T> : DomainEventHandlerBase<T>
    where T : DomainEvent
{
    private readonly IMessageSender _publisher;

    public DomainEventPropagator(IMessageSender publisher)
    {
        _publisher = publisher;
    }

    protected override async Task<Response<Nothing>> Handle(T ev)
    {
        await _publisher.Publish(ConvertToMessage(ev));
        return Ok;
    }

    protected abstract IMessage ConvertToMessage(T ev);
}
