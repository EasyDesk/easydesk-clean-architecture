using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Tools.Options;
using MediatR;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Mediator;

public interface IEventContext : INotification
{
    Option<Error> Error { get; }

    void SetError(Error error);
}

public class EventContext<T> : IEventContext
{
    public EventContext(T eventData)
    {
        EventData = eventData;
    }

    public T EventData { get; }

    public Option<Error> Error { get; private set; } = None;

    public void SetError(Error error)
    {
        Error = Some(error);
    }
}
