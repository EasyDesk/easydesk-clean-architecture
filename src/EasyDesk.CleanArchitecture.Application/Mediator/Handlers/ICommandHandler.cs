namespace EasyDesk.CleanArchitecture.Application.Mediator.Handlers;

public interface ICommandHandler<TRequest, TResponse> : IRequestHandlerWithResponse<TRequest, TResponse>
    where TRequest : CommandBase<TResponse>
{
}
