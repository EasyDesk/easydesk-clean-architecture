using EasyDesk.Commons.Options;
using Rebus.Messages;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

public interface IOutbox
{
    void EnqueueMessageForStorage(TransportMessage message, string destinationAddress);

    Task StoreEnqueuedMessages();

    Task<IEnumerable<(TransportMessage Message, string DestinationAddress)>> RetrieveNextMessages(Option<int> count);
}
