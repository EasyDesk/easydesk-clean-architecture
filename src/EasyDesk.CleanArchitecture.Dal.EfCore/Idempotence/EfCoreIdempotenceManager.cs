using EasyDesk.CleanArchitecture.Application.Messaging.Idempotence;
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

    public async Task<bool> HasBeenProcessed(string messageId)
    {
        await _unitOfWork.RegisterExternalDbContext(_idempotenceContext);
        return await _idempotenceContext
            .HandledMessages
            .AnyAsync(e => e.Id == messageId);
    }

    public async Task MarkAsProcessed(string messageId)
    {
        await _unitOfWork.RegisterExternalDbContext(_idempotenceContext);
        _idempotenceContext.HandledMessages.Add(new HandledMessage
        {
            Id = messageId
        });
        await _idempotenceContext.SaveChangesAsync();
    }
}
