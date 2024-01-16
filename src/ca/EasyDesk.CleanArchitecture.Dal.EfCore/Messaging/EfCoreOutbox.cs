using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;
using Rebus.Messages;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;

internal class EfCoreOutbox : IOutbox
{
    public static readonly Duration MessageAgingTime = Duration.FromSeconds(30);

    private readonly MessagingContext _context;
    private readonly JsonSerializer _serializer;

    public EfCoreOutbox(MessagingContext context, JsonSettingsConfigurator jsonSettingsConfigurator)
    {
        _context = context;
        _serializer = JsonSerializer.Create(jsonSettingsConfigurator.CreateSettings());
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

    public async Task<IEnumerable<(TransportMessage, string)>> RetrieveNextMessages(Option<int> count)
    {
        var outboxMessages = await _context.Outbox
            .OrderBy(m => m.Id)
            .Conditionally(count, c => q => q.Take(c))
            .ToListAsync();

        var messages = outboxMessages
            .Select(m => (ToTransportMessage(m), m.DestinationAddress))
            .ToList();

        if (messages.HasAny())
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
