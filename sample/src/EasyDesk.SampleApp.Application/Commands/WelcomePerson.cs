using EasyDesk.CleanArchitecture.Application.Messaging;
using Microsoft.Extensions.Logging;

namespace EasyDesk.SampleApp.Application.Commands;

public record WelcomePerson(string Name) : IOutgoingCommand, IIncomingCommand;

public class HandleWelcome : IMessageHandler<WelcomePerson>
{
    private readonly ILogger<HandleWelcome> _logger;

    public HandleWelcome(ILogger<HandleWelcome> logger)
    {
        _logger = logger;
    }

    public Task<Result<Nothing>> Handle(WelcomePerson message)
    {
        _logger.LogInformation("Welcome {personName}", message.Name);
        return Task.FromResult(Ok);
    }
}
