using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Idempotence;

public interface IIdempotenceManager
{
    Task<bool> HasBeenProcessed(string messageId);

    Task MarkAsProcessed(string messageId);
}
