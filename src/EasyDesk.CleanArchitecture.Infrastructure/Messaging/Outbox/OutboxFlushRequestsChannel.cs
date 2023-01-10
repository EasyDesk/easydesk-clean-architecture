using Rebus.Transport;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

internal class OutboxFlushRequestsChannel
{
    private readonly Channel<ITransport> _channel;

    public OutboxFlushRequestsChannel()
    {
        _channel = Channel.CreateUnbounded<ITransport>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public void RequestNewFlush(ITransport transport) => _channel.Writer.TryWrite(transport);

    public IAsyncEnumerable<ITransport> GetAllFlushRequests(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
