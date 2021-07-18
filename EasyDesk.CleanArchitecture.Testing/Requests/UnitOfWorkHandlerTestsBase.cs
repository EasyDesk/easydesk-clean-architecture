using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Infrastructure.UnitOfWork;

namespace EasyDesk.Testing.Utils.Requests
{
    public abstract class UnitOfWorkHandlerTestsBase<THandler, TRequest, TResponse> : RequestHandlerTestsBase<THandler, TRequest, TResponse>
        where THandler : UnitOfWorkHandler<TRequest, TResponse>
        where TRequest : CommandBase<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork = new NoUnitOfWork();

        protected override THandler CreateHandler() => CreateHandler(_unitOfWork);

        protected abstract THandler CreateHandler(IUnitOfWork unitOfWork);
    }
}
