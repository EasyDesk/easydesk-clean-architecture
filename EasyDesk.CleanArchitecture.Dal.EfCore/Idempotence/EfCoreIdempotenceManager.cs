using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Idempotence;
using EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence
{
    public class EfCoreIdempotenceManager : IIdempotenceManager
    {
        private readonly IdempotenceContext _idempotenceContext;
        private readonly EfCoreUnitOfWork _unitOfWork;

        public EfCoreIdempotenceManager(IdempotenceContext idempotenceContext, EfCoreUnitOfWork unitOfWork)
        {
            _idempotenceContext = idempotenceContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> HasBeenProcessed(EventBusMessage message)
{
            await _unitOfWork.RegisterExternalDbContext(_idempotenceContext);
            return await _idempotenceContext
                .HandledEvents
                .AnyAsync(e => e.Id == message.Id);
        }

        public async Task MarkAsProcessed(EventBusMessage message)
        {
            await _unitOfWork.RegisterExternalDbContext(_idempotenceContext);
            _idempotenceContext.HandledEvents.Add(new HandledEvent
            {
                Id = message.Id
            });
            await _idempotenceContext.SaveChangesAsync();
        }
    }
}
