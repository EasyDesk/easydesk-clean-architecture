using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus
{
    public interface IEventBusPublisher : IAsyncDisposable
    {
        Task Publish(IEnumerable<EventBusMessage> messages);
    }
}
