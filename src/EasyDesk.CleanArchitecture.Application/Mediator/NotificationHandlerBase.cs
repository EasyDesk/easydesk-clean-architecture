using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Application.Mediator;

public abstract class NotificationHandlerBase<T> : INotificationHandler<EventContext<T>>
{
    public async Task Handle(EventContext<T> context, CancellationToken cancellationToken)
    {
        if (context.Error.IsPresent)
        {
            return;
        }

        await Handle(context.EventData)
            .ThenIfFailure(context.SetError);
    }

    protected abstract Task<Response<Nothing>> Handle(T ev);
}
