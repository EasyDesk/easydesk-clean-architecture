using System;
using System.Collections.Generic;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Sender.Outbox;

public interface IOutboxChannel
{
    void OnNewMessageGroup(IEnumerable<Guid> messageIds);

    IAsyncEnumerable<IEnumerable<Guid>> GetAllMessageGroups();
}

public class OutboxChannel : IOutboxChannel
{
    private readonly Channel<IEnumerable<Guid>> _channel;

    public OutboxChannel()
    {
        _channel = Channel.CreateUnbounded<IEnumerable<Guid>>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public void OnNewMessageGroup(IEnumerable<Guid> messageIds) =>
        _channel.Writer.TryWrite(messageIds);

    public IAsyncEnumerable<IEnumerable<Guid>> GetAllMessageGroups() =>
        _channel.Reader.ReadAllAsync();
}
