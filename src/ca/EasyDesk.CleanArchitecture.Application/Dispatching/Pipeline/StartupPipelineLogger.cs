using Autofac;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.Commons.Collections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

public sealed class StartupPipelineLogger : IHostedService
{
    private readonly IEnumerable<Type> _stepTypes;
    private readonly ILogger<StartupPipelineLogger> _logger;
    private readonly ILifetimeScope _lifetimeScope;

    public StartupPipelineLogger(IEnumerable<Type> steps, ILogger<StartupPipelineLogger> logger, ILifetimeScope lifetimeScope)
    {
        _stepTypes = steps;
        _logger = logger;
        _lifetimeScope = lifetimeScope;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LogPipeline(_stepTypes, "Static pipeline");
        await using (var scope = _lifetimeScope.BeginUseCaseLifetimeScope())
        {
            var requestSteps = scope.Resolve<IPipelineProvider>().GetSteps<ICommandRequest<Nothing>, Nothing>(scope);
            LogPipeline(requestSteps.Select(s => s.GetType()), "Command request pipeline");
            LogPipeline(requestSteps.Where(s => s.IsForEachHandler).Select(s => s.GetType()), "Nested command request pipeline");
            var messageSteps = scope.Resolve<IPipelineProvider>().GetSteps<IIncomingCommand, Nothing>(scope);
            LogPipeline(messageSteps.Select(s => s.GetType()), "Command message pipeline");
            LogPipeline(messageSteps.Where(s => s.IsForEachHandler).Select(s => s.GetType()), "Nested command message pipeline");
        }
    }

    private void LogPipeline(IEnumerable<Type> steps, string name)
    {
        var stepsList = steps.Select((s, i) => $"{i + 1,3}. {s.Namespace}.{s.Name.Split('`')[0]}").ToList().AsEnumerable();
        _logger.LogInformation(
            """

            -------------------------------------------------------------------------------
            {SpacesForName}{Name}
            -------------------------------------------------------------------------------
            {StepsList}
            ----------------------------------- Handler -----------------------------------
            {ReversedStepsList}
            -------------------------------------------------------------------------------
            """,
            new string(' ', 39 - name.Length / 2),
            name,
            stepsList.ConcatStrings("\n"),
            stepsList.Reverse().ConcatStrings("\n"));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
