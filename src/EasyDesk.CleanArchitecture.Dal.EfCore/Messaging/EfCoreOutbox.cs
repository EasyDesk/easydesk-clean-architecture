using EasyDesk.CleanArchitecture.Application.Messaging.Outbox;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Infrastructure.Json;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;
using Rebus.Messages;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;

public class EfCoreOutbox : IOutbox
{
    public static readonly Duration MessageAgingTime = Duration.FromSeconds(30);

    private readonly MessagingContext _context;
    private readonly JsonSerializer _serializer;

    public EfCoreOutbox(MessagingContext context)
    {
        _context = context;
        _serializer = JsonSerializer.Create(JsonDefaults.DefaultSerializerSettings());
    }

    public void EnqueueMessageForStorage(TransportMessage message, string destinationAddress)
    {
        var outboxMessage = new OutboxMessage
        {
            Content = message.Body,
            Headers = _serializer.SerializeToBsonBytes(message.Headers),
            DestinationAddress = destinationAddress
        };
        _context.Outbox.Add(outboxMessage);
    }

    public async Task StoreEnqueuedMessages()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<(TransportMessage, string)>> RetrieveNextMessages(int count)
    {
        var outboxMessages = await _context.Outbox
            .OrderBy(m => m.Id)
            .Take(count)
            .ToListAsync();

        var messages = outboxMessages.Select(m => (ToTransportMessage(m), m.DestinationAddress));

        if (messages.Any())
        {
            _context.Outbox.RemoveRange(outboxMessages);
            await _context.SaveChangesAsync();
        }

        return messages;
    }

    private TransportMessage ToTransportMessage(OutboxMessage outboxMessage)
    {
        var headers = _serializer.DeserializeFromBsonBytes<Dictionary<string, string>>(outboxMessage.Headers);
        return new TransportMessage(headers, outboxMessage.Content);
    }
}
