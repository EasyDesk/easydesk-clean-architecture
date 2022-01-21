using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker.Idempotence;

public interface IIdempotenceManager
{
    Task<bool> HasBeenProcessed(Message message);

    Task MarkAsProcessed(Message message);
}
