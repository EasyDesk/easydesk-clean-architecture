namespace EasyDesk.CleanArchitecture.Application.Cqrs.Async;

public interface IEvent : IMessage, IReadWriteOperation
{
}
