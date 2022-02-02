using Rebus.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Outbox;

public interface IOutbox
{
    void EnqueueMessageForStorage(TransportMessage message, string destinationAddress);

    Task StoreEnqueuedMessages();

    Task<IEnumerable<(TransportMessage Message, string DestinationAddress)>> RetrieveNextMessages(int count);
}
