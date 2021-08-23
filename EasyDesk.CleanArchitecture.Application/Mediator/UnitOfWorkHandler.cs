using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Responses;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Mediator
{
    public abstract class UnitOfWorkHandler<TRequest, TResponse> : RequestHandlerBase<TRequest, TResponse>
        where TRequest : CommandBase<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UnitOfWorkHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected sealed override async Task<Response<TResponse>> Handle(TRequest request)
        {
            return await HandleRequest(request)
                .ThenRequireAsync(_ => _unitOfWork.Save());
        }

        protected abstract Task<Response<TResponse>> HandleRequest(TRequest request);
    }
}
