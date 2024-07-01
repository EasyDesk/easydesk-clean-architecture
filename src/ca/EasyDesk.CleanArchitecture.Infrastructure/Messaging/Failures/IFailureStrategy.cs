using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.Commons.Tasks;
using Rebus.Retry.Simple;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Failures;

public interface IFailureStrategy
{
    Task Handle<T>(IFailed<T> message, AsyncAction next) where T : IIncomingMessage;
}
