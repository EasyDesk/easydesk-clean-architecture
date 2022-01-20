using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus;

public record EventBusConsumerDefinition(IEnumerable<string> EventTypes);
