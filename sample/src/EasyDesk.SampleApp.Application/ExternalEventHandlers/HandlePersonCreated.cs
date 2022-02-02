using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.SampleApp.Application.DomainEventHandlers.PropagatedEvents;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EasyDesk.SampleApp.Application.ExternalEventHandlers;

public class HandlePersonCreated : IMessageHandler<PersonCreated>
{
    private readonly ILogger<HandlePersonCreated> _logger;

    public HandlePersonCreated(ILogger<HandlePersonCreated> logger)
    {
        _logger = logger;
    }

    public Task Handle(PersonCreated message)
    {
        _logger.LogInformation("{message}", message);
        return Task.CompletedTask;
    }
}
