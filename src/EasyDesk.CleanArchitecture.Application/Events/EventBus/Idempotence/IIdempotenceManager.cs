using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus.Idempotence
{
    public interface IIdempotenceManager
    {
        Task<bool> HasBeenProcessed(EventBusMessage message);

        Task MarkAsProcessed(EventBusMessage message);
    }
}
