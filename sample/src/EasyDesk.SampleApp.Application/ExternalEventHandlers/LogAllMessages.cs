using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.Tools;
using EasyDesk.Tools.Results;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static EasyDesk.Tools.Results.ResultImports;

namespace EasyDesk.SampleApp.Application.ExternalEventHandlers;

public class LogAllMessages : IMessageHandler<IMessage>
{
    private readonly ILogger<LogAllMessages> _logger;

    public LogAllMessages(ILogger<LogAllMessages> logger)
    {
        _logger = logger;
    }

    public Task<Result<Nothing>> Handle(IMessage message)
    {
        _logger.LogInformation("Message received: {message}", message);
        return Task.FromResult(Ok);
    }
}
