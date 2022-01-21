using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker;

public record MessageConsumerDefinition(IEnumerable<string> SupportedTypes);
