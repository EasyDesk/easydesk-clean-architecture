using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.Tools;
using EasyDesk.Tools.Results;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static EasyDesk.Tools.Results.ResultImports;

namespace EasyDesk.SampleApp.Application.ExternalEventHandlers;

public class HandleAllMessages : IMessageHandler<IMessage>
{
    private readonly ILogger<HandleAllMessages> _logger;

    public HandleAllMessages(ILogger<HandleAllMessages> logger)
    {
        _logger = logger;
    }

    public Task<Result<Nothing>> Handle(IMessage message)
    {
        _logger.LogInformation("{message}", message);
        return Task.FromResult(Ok);
    }
}
