using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using System.Collections.Generic;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel
{
    public interface IDomainEventBuffer
    {
        Option<IDomainEvent> ConsumeEvent();
    }

    public static class DomainEventBufferExtensions
    {
        public static IEnumerable<IDomainEvent> ConsumeAllEvents(this IDomainEventBuffer eventBuffer)
        {
            return EnumerableUtils.Generate(eventBuffer.ConsumeEvent)
                .TakeWhile(ev => ev.IsPresent)
                .Select(ev => ev.Value);
        }
    }
}
