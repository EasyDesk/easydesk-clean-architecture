using AutoMapper;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Web.Controllers
{
    public abstract partial class AbstractMediatrController : AbstractController
    {
        private IMediator _mediator;
        private IMapper _mapper;
        private readonly ITransactionManager _transactionManager;

        protected IMediator Mediator => _mediator ??= GetService<IMediator>();

        protected IMapper Mapper => _mapper ??= GetService<IMapper>();

        protected ITransactionManager TransactionManager => _transactionManager ?? GetService<ITransactionManager>();

        private T GetService<T>() => HttpContext.RequestServices.GetRequiredService<T>();

        protected ResultBuilder<TResponse> Command<TResponse>(CommandBase<TResponse> command, bool transactional = true) =>
            MakeRequest(() => RunCommand(command, transactional));

        private async Task<Response<TResponse>> RunCommand<TResponse>(CommandBase<TResponse> command, bool transactional)
        {
            if (transactional)
            {
                return await TransactionManager.RunTransactionally(() => DefaultSend(command));
            }
            return await DefaultSend(command);
        }

        protected ResultBuilder<TResponse> Query<TResponse>(QueryBase<TResponse> query) =>
            MakeRequest(() => DefaultSend(query));

        private ResultBuilder<TResponse> MakeRequest<TResponse>(AsyncFunc<Response<TResponse>> request)
        {
            return new(request, this, Mapper);
        }

        private async Task<Response<T>> DefaultSend<T>(RequestBase<T> request) => await Mediator.Send(request);
    }
}
