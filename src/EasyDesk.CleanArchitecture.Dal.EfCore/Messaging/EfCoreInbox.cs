using EasyDesk.CleanArchitecture.Application.Messaging.Inbox;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;

internal class EfCoreInbox : IInbox
{
    private readonly MessagingContext _context;

    public EfCoreInbox(MessagingContext context)
    {
        _context = context;
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
            Id = messageId
        });
        await _context.SaveChangesAsync();
    }
}
