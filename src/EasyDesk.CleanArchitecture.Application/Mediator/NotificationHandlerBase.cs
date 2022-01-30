using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using EasyDesk.Tools.Options;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Mediator;

public abstract class NotificationHandlerBase<T> : INotificationHandler<EventContext<T>>
{
    private readonly Option<IUnitOfWork> _unitOfWork;

    protected NotificationHandlerBase(IUnitOfWork unitOfWork) : this(Some(unitOfWork))
    {
    }

    protected NotificationHandlerBase() : this(None)
    {
    }

    private NotificationHandlerBase(Option<IUnitOfWork> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(EventContext<T> context, CancellationToken cancellationToken)
    {
        if (context.Error.IsPresent)
        {
            return;
        }

        await Handle(context.EventData)
            .ThenIfSuccessAsync(_ => Save())
            .ThenIfFailure(context.SetError);
    }

    private async Task Save()
    {
        if (_unitOfWork.IsPresent)
        {
            await _unitOfWork.Value.Save();
        }
    }

    protected abstract Task<Response<Nothing>> Handle(T ev);
}
