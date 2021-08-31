using AutoMapper;
using EasyDesk.CleanArchitecture.Application.Mediator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Controllers
{
    public abstract partial class AbstractMediatrController : AbstractController
    {
        private IMediator _mediator;
        private IMapper _mapper;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
        protected IMapper Mapper => _mapper ??= HttpContext.RequestServices.GetRequiredService<IMapper>();

        protected ResultBuilder<TResponse> Send<TResponse>(RequestBase<TResponse> request)
        {
            return new(() => Mediator.Send(request), this, Mapper);
        }
    }
}