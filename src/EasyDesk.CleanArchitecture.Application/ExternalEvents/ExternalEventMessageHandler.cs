using System.Threading.Tasks;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Receiver;

namespace EasyDesk.CleanArchitecture.Application.ExternalEvents;

public class ExternalEventMessageHandler : IMessageHandler
{
    private readonly IExternalEventHandler _externalEventHandler;

    public ExternalEventMessageHandler(IExternalEventHandler externalEventHandler)
    {
        _externalEventHandler = externalEventHandler;
    }

    public async Task<MessageHandlerResult> Handle(Message message) => message.Content switch
    {
        ExternalEvent externalEvent => await HandleExternalEvent(externalEvent),
        _ => MessageHandlerResult.NotSupported
    };

    private async Task<MessageHandlerResult> HandleExternalEvent(ExternalEvent externalEvent)
    {
        var handlerResponse = await _externalEventHandler.Handle(externalEvent);
        return handlerResponse.Match(
            success: _ => MessageHandlerResult.Handled,
            failure: _ => MessageHandlerResult.GenericFailure);
    }
}
