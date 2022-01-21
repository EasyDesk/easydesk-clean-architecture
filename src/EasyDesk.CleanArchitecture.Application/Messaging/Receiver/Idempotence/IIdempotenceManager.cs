using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Receiver.Idempotence;

public interface IIdempotenceManager
{
    Task<bool> HasBeenProcessed(Message message);

    Task MarkAsProcessed(Message message);
}
