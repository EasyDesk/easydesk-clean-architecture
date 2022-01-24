using EasyDesk.CleanArchitecture.Application.Messaging;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Receiver.Idempotence;

public interface IIdempotenceManager
{
    Task<bool> HasBeenProcessed(Message message);

    Task MarkAsProcessed(Message message);
}
