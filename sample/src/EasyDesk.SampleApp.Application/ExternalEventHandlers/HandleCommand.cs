using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.SampleApp.Application.DomainEventHandlers;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EasyDesk.SampleApp.Application.ExternalEventHandlers;

public class HandleCommand : IMessageHandler<SendPersonCreatedEmail>
{
    private readonly ILogger<HandleCommand> _logger;

    public HandleCommand(ILogger<HandleCommand> logger)
    {
        _logger = logger;
    }

    public Task Handle(SendPersonCreatedEmail message)
    {
        _logger.LogInformation("{message} received", message);
        return Task.CompletedTask;
    }
}
