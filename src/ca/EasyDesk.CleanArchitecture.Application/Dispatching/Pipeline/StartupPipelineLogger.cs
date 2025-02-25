using EasyDesk.Commons.Collections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

public sealed class StartupPipelineLogger : IHostedService
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
        LogPipeline(_steps, "Main use case pipeline");
        return Task.CompletedTask;
    }

    private void LogPipeline(IEnumerable<Type> steps, string name)
    {
        var stepsList = steps.Select((s, i) => $"{i + 1,3}. {s.Namespace}.{s.Name.Split('`')[0]}").ToList().AsEnumerable();
        _logger.LogInformation(
            """

            -------------------------------------------------------------------------------
            {spacesForName}{name}
            -------------------------------------------------------------------------------
            {stepsList}
            ----------------------------------- Handler -----------------------------------
            {reversedStepsList}
            -------------------------------------------------------------------------------
            """,
            new string(' ', 39 - name.Length / 2),
            name,
            stepsList.ConcatStrings("\n"),
            stepsList.Reverse().ConcatStrings("\n"));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
