using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.ExternalEvents
{
    public interface IExternalEventPublisher
    {
        Task Publish(IEnumerable<ExternalEvent> events);
    }

    public static class EventPublisherExtensions
    {
        public static Task Publish(this IExternalEventPublisher publisher, params ExternalEvent[] events) =>
            publisher.Publish(events);
    }
}
