using Autofac;
using Rebus.Messages;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

internal class TransportWithOutbox : ITransport
{
    public const string UseOutboxHeader = "x-use-outbox";

    private readonly ITransport _transport;

    public TransportWithOutbox(ITransport transport)
    {
        _transport = transport;
    }

    public void CreateQueue(string address) => _transport.CreateQueue(address);

    public async Task Send(string destinationAddress, TransportMessage message, ITransactionContext context)
    {
        var shouldUseOutbox = message.Headers.ContainsKey(UseOutboxHeader);
        if (!shouldUseOutbox)
        {
            await _transport.Send(destinationAddress, message, context);
            return;
        }

        message.Headers.Remove(UseOutboxHeader);

        var componentContext = context.GetComponentContext();
        var outbox = componentContext.Resolve<IOutbox>();
        outbox.EnqueueMessageForStorage(message, destinationAddress);

        var helper = componentContext.Resolve<OutboxTransactionHelper>();
        helper.EnsureCommitHooksAreRegistered();
    }

    public Task<TransportMessage> Receive(ITransactionContext context, CancellationToken cancellationToken) =>
        _transport.Receive(context, cancellationToken);

    public string Address => _transport.Address;
}
