using EasyDesk.Tools.Results;
using MediatR;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Handlers;

public interface IRequestHandlerWithResponse<TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse>>
    where TRequest : RequestBase<TResponse>
{
}
