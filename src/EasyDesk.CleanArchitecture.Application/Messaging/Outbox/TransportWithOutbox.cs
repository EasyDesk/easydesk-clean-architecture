using EasyDesk.CleanArchitecture.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Messages;
using Rebus.Transport;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Outbox;

public class TransportWithOutbox : ITransport
{
    private readonly ITransport _transport;

    public TransportWithOutbox(ITransport transport)
    {
        _transport = transport;
    }

    public void CreateQueue(string address) => _transport.CreateQueue(address);

    public Task Send(string destinationAddress, TransportMessage message, ITransactionContext context)
    {
        var serviceProvider = context.GetServiceProvider();
        var outbox = serviceProvider.GetRequiredService<IOutbox>();
        outbox.EnqueueMessageForStorage(message, destinationAddress);

        var helper = serviceProvider.GetRequiredService<OutboxTransactionHelper>();
        helper.EnsureCommitHooksAreRegistered();

        return Task.CompletedTask;
    }

    public Task<TransportMessage> Receive(ITransactionContext context, CancellationToken cancellationToken) =>
        _transport.Receive(context, cancellationToken);

    public string Address => _transport.Address;
}
