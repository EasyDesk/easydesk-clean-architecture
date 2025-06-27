using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IFailedMessageHandler<T>
    where T : IIncomingMessage
{
    Task<Result<Nothing>> HandleFailure(T message);
}
