using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Rebus.Messages;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;

internal class EfCoreOutbox : IOutbox
{
    public static readonly Duration MessageAgingTime = Duration.FromSeconds(30);

    private readonly MessagingContext _context;
    private readonly JsonSerializerOptions _jsonOptions;

    public EfCoreOutbox(MessagingContext context, JsonOptionsConfigurator jsonSettingsConfigurator)
    {
        _context = context;
        _jsonOptions = jsonSettingsConfigurator.CreateOptions();
    }

    public void EnqueueMessageForStorage(TransportMessage message, string destinationAddress)
    {
        var outboxMessage = new OutboxMessage
        {
            Content = message.Body,
            Headers = JsonSerializer.Serialize(message.Headers, _jsonOptions),
            DestinationAddress = destinationAddress,
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
            .Conditionally(count, (q, c) => q.Take(c))
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
        var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(outboxMessage.Headers, _jsonOptions);
        return new(headers, outboxMessage.Content);
    }
}
