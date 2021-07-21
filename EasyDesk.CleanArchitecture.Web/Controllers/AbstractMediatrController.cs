using AutoMapper;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.UserInfo;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Controllers
{
    public abstract partial class AbstractMediatrController : AbstractController
    {
        private IMediator _mediator;
        private IMapper _mapper;
        private IUserInfo _userInfo;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
        protected IMapper Mapper => _mapper ??= HttpContext.RequestServices.GetRequiredService<IMapper>();
        protected IUserInfo UserInfo => _userInfo ??= HttpContext.RequestServices.GetRequiredService<IUserInfo>();

        protected ResultBuilder<TResponse> Send<TResponse>(RequestBase<TResponse> request)
        {
            return new(() => Mediator.SendRequestWithContext(request, UserInfo), this, Mapper);
        }
    }
}