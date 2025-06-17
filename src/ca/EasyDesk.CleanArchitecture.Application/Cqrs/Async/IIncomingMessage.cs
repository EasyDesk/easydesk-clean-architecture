using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.Async;

public interface IIncomingMessage : IMessage, IDispatchable<Nothing>;
