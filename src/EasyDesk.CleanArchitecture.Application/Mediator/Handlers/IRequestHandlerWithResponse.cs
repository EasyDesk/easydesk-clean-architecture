using EasyDesk.CleanArchitecture.Application.Responses;
using MediatR;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Handlers;

public interface IRequestHandlerWithResponse<TRequest, TResponse> : IRequestHandler<TRequest, Response<TResponse>>
    where TRequest : RequestBase<TResponse>
{
}
