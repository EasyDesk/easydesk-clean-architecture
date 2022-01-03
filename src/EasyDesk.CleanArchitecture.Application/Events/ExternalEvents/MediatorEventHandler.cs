using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using MediatR;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;

public class MediatorEventHandler : IExternalEventHandler
{
    private readonly IMediator _mediator;

    public MediatorEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Response<Nothing>> Handle(ExternalEvent ev)
    {
        return await _mediator.PublishEvent(ev);
    }
}
