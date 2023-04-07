using EasyDesk.Commons.Collections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

public class StartupPipelineLogger : IHostedService
{
    private readonly IEnumerable<Type> _steps;
    private readonly ILogger<StartupPipelineLogger> _logger;

    public StartupPipelineLogger(IEnumerable<Type> steps, ILogger<StartupPipelineLogger> logger)
    {
        _steps = steps;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var stepsList = _steps.Select((s, i) => $"{i + 1,3}. {s.Namespace}.{s.Name.Split('`')[0]}").ToList().AsEnumerable();
        _logger.LogInformation(
            """

            -------------------------------------------------------------------------------
                                           Request pipeline
            -------------------------------------------------------------------------------
            {stepsList}
            ----------------------------------- Handler -----------------------------------
            {reversedStepsList}
            -------------------------------------------------------------------------------
            """,
            stepsList.ConcatStrings("\n"),
            stepsList.Reverse().ConcatStrings("\n"));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
