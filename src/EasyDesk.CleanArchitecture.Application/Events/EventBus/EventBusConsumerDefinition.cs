using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;

public record EventBusConsumerDefinition(IEnumerable<string> EventTypes);
