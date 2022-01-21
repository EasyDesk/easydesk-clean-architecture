using EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker;
using EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker.Idempotence;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence;

public class EfCoreIdempotenceManager : IIdempotenceManager
{
    private readonly IdempotenceContext _idempotenceContext;
    private readonly EfCoreTransactionManager _unitOfWork;

    public EfCoreIdempotenceManager(IdempotenceContext idempotenceContext, EfCoreTransactionManager unitOfWork)
    {
        _idempotenceContext = idempotenceContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HasBeenProcessed(Message message)
    {
        await _unitOfWork.RegisterExternalDbContext(_idempotenceContext);
        return await _idempotenceContext
            .HandledMessages
            .AnyAsync(e => e.Id == message.Id);
    }

    public async Task MarkAsProcessed(Message message)
    {
        await _unitOfWork.RegisterExternalDbContext(_idempotenceContext);
        _idempotenceContext.HandledMessages.Add(new HandledMessage
        {
            Id = message.Id
        });
        await _idempotenceContext.SaveChangesAsync();
    }
}
