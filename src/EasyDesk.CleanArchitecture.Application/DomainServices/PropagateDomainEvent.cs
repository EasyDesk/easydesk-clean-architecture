using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools;
using EasyDesk.Tools.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EasyDesk.Tools.Results.ResultImports;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public interface IDomainEventPropagator<T>
{
    IMessage ConvertToMessage(T ev);
}

public class PropagateDomainEvent<T> : IDomainEventHandler<T>
    where T : DomainEvent
{
    private readonly IMessagePublisher _publisher;
    private readonly IEnumerable<IDomainEventPropagator<T>> _propagators;

    public PropagateDomainEvent(IMessagePublisher publisher, IEnumerable<IDomainEventPropagator<T>> propagators)
    {
        _publisher = publisher;
        _propagators = propagators;
    }

    public async Task<Result<Nothing>> Handle(T ev)
    {
        var messages = _propagators.Select(p => p.ConvertToMessage(ev));
        foreach (var message in messages)
        {
            await _publisher.Publish(message);
        }
        return Ok;
    }
}
