using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Receiver;

public record MessageReceiverDefinition(IEnumerable<string> SupportedTypes);
