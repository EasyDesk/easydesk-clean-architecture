using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Receiver;

public record MessageReceiverDefinition(IEnumerable<string> SupportedTypes);
