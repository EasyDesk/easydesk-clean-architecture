using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.ExternalEvents
{
    public interface IExternalEventPublisher
    {
        Task Publish(IEnumerable<IExternalEvent> events);
    }

    public static class EventPublisherExtensions
    {
        public static Task Publish(this IExternalEventPublisher publisher, params IExternalEvent[] events) =>
            publisher.Publish(events);
    }
}
