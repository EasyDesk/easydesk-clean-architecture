using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Results;
using EasyDesk.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Domain.Metamodel.Results.ResultImports;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Handlers;

public interface IDomainEventPropagator<T>
{
    IMessage ConvertToMessage(T ev);
}

public class PropagateDomainEvent<T> : IDomainEventHandler<T>
    where T : DomainEvent
{
    private readonly MessageBroker _broker;
    private readonly IEnumerable<IDomainEventPropagator<T>> _propagators;

    public PropagateDomainEvent(MessageBroker broker, IEnumerable<IDomainEventPropagator<T>> propagators)
    {
        _broker = broker;
        _propagators = propagators;
    }

    public async Task<Result<Nothing>> Handle(T ev)
    {
        var messages = _propagators.Select(p => p.ConvertToMessage(ev));
        foreach (var message in messages)
        {
            await _broker.Publish(message);
        }
        return Ok;
    }
}
