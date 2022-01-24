using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Sender;
using EasyDesk.Tools.Options;

namespace EasyDesk.CleanArchitecture.Application.ExternalEvents;

public class ExternalEventPublisher : IExternalEventPublisher
{
    private readonly IMessageSender _sender;

    public ExternalEventPublisher(IMessageSender sender)
    {
        _sender = sender;
    }

    public async Task Publish(IEnumerable<ExternalEvent> events)
    {
        var messages = events.Select(e => Message.Create(e)).ToList();
        await _sender.Send(messages);
    }
}
