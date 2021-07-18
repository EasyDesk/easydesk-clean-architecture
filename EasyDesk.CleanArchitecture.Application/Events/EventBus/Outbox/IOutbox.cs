using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus.Outbox
{
    public interface IOutbox
    {
        Task StoreMessages(IEnumerable<EventBusMessage> messages);

        Task PublishMessages(IEnumerable<Guid> messageIds);

        Task Flush();
    }
}
