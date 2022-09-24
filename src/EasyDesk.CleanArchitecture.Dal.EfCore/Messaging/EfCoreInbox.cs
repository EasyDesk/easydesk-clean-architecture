using EasyDesk.CleanArchitecture.Application.Messaging.Inbox;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;

public class EfCoreInbox : IInbox
{
    private readonly MessagingContext _context;
    private readonly EfCoreUnitOfWorkProvider _unitOfWorkProvider;

    public EfCoreInbox(MessagingContext context, EfCoreUnitOfWorkProvider unitOfWorkProvider)
    {
        _context = context;
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    public async Task<bool> HasBeenProcessed(string messageId)
    {
        await _unitOfWorkProvider.RegisterExternalDbContext(_context);
        return await _context
            .Inbox
            .AnyAsync(e => e.Id == messageId);
    }

    public async Task MarkAsProcessed(string messageId)
    {
        await _unitOfWorkProvider.RegisterExternalDbContext(_context);
        _context.Inbox.Add(new InboxMessage
        {
            Id = messageId
        });
        await _context.SaveChangesAsync();
    }
}
