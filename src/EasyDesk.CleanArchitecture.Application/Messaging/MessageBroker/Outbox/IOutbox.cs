using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker.Outbox;

public interface IOutbox
{
    Task StoreMessages(IEnumerable<Message> messages);

    Task PublishMessages(IEnumerable<Guid> messageIds);

    Task Flush();
}
