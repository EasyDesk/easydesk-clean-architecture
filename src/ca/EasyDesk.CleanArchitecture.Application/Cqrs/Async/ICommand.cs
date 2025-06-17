namespace EasyDesk.CleanArchitecture.Application.Cqrs.Async;

public interface ICommand : IMessage, IReadWriteOperation;
