namespace EasyDesk.CleanArchitecture.Application.Cqrs.Handlers;

public interface ICommandHandler<TCommand, TResult> : ICqrsRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
}
