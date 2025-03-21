﻿using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

public class OutboxFlushRequestsChannel
{
    private readonly Channel<Nothing> _channel;

    public OutboxFlushRequestsChannel()
    {
        _channel = Channel.CreateUnbounded<Nothing>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });
    }

    public void RequestNewFlush() => _channel.Writer.TryWrite(Nothing.Value);

    public IAsyncEnumerable<Nothing> GetAllFlushRequests(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
