using EasyDesk.CleanArchitecture.Application.Messaging.Outbox;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Infrastructure.Json;
using EasyDesk.Tools.Options;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;
using Rebus.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Outbox;

public class EfCoreOutbox : IOutbox
{
    public static readonly Duration MessageAgingTime = Duration.FromSeconds(30);

    private readonly OutboxContext _outboxContext;
    private readonly EfCoreUnitOfWorkProvider _unitOfWorkProvider;
    private readonly JsonSerializer _serializer;

    public EfCoreOutbox(
        OutboxContext outboxContext,
        EfCoreUnitOfWorkProvider unitOfWorkProvider)
    {
        _outboxContext = outboxContext;
        _unitOfWorkProvider = unitOfWorkProvider;
        _serializer = JsonSerializer.Create(JsonDefaults.DefaultSerializerSettings());
    }

    public void EnqueueMessageForStorage(TransportMessage message, string destinationAddress)
    {
        var outboxMessages = new OutboxMessage
        {
            Content = message.Body,
            Headers = _serializer.SerializeToBsonBytes(message.Headers),
            DestinationAddress = destinationAddress
        };
        _outboxContext.Messages.AddRange(outboxMessages);
    }

    public async Task StoreEnqueuedMessages()
    {
        await _unitOfWorkProvider.RegisterExternalDbContext(_outboxContext);
        await _outboxContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<(TransportMessage, string)>> RetrieveNextMessages(int count)
    {
        await _unitOfWorkProvider.RegisterExternalDbContext(_outboxContext);

        var outboxMessages = await _outboxContext.Messages
            .OrderBy(m => m.Id)
            .Take(count)
            .ToListAsync();

        var messages = outboxMessages.Select(m => (ToTransportMessage(m), m.DestinationAddress));

        if (messages.Any())
        {
            _outboxContext.Messages.RemoveRange(outboxMessages);
            await _outboxContext.SaveChangesAsync();
        }

        return messages;
    }

    private TransportMessage ToTransportMessage(OutboxMessage outboxMessage)
    {
        var headers = _serializer.DeserializeFromBsonBytes<Dictionary<string, string>>(outboxMessage.Headers);
        return new TransportMessage(headers, outboxMessage.Content);
    }
}
