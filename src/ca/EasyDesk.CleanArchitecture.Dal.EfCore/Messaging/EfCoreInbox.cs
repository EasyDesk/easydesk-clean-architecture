using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;

internal class EfCoreInbox : IInbox
{
    private readonly MessagingContext _context;
    private readonly IClock _clock;

    public EfCoreInbox(
        MessagingContext context,
        IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<bool> HasBeenProcessed(string messageId)
    {
        return await _context
            .Inbox
            .AnyAsync(e => e.Id == messageId);
    }

    public async Task MarkAsProcessed(string messageId)
    {
        _context.Inbox.Add(new InboxMessage
        {
            Id = messageId,
            Instant = _clock.GetCurrentInstant(),
        });
        await _context.SaveChangesAsync();
    }
}
