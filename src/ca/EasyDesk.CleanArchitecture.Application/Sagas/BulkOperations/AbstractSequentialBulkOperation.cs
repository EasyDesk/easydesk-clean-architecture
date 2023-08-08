using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Messaging;

namespace EasyDesk.CleanArchitecture.Application.Sagas.BulkOperations;

public abstract record AbstractBulkOperationCommand : IIncomingCommand, IOutgoingCommand
{
    public static string GetDestination(RoutingContext context) => context.Self;
}

public record BulkOperationState<R, S>(R Result, S RemainingWork);

public abstract class AbstractSequentialBulkOperation<TSelf, TStartCommand, TResult, TWork, TBatchCommand> :
    ISagaController<string, BulkOperationState<TResult, TWork>>
    where TStartCommand : IDispatchable<TResult>
    where TBatchCommand : AbstractBulkOperationCommand
    where TSelf : AbstractSequentialBulkOperation<TSelf, TStartCommand, TResult, TWork, TBatchCommand>
{
    private readonly ICommandSender _commandSender;

    public AbstractSequentialBulkOperation(ICommandSender commandSender)
    {
        _commandSender = commandSender;
    }

    private async Task<Result<TResult>> Start(SagaContext<string, BulkOperationState<TResult, TWork>> context)
    {
        if (!context.IsNew)
        {
            return Errors.Generic("Another bulk operation of type {operationType} is already in progress.", typeof(TSelf).Name);
        }
        return await RequestNextBatchComputation(context);
    }

    private async Task<Result<Nothing>> Handle(TBatchCommand command, SagaContext<string, BulkOperationState<TResult, TWork>> context)
    {
        var remainingWork = await HandleBatch(command, context.State.RemainingWork);
        context.MutateState(s => s with { RemainingWork = remainingWork });
        return await RequestNextBatchComputation(context);
    }

    private async Task<Result<TResult>> RequestNextBatchComputation(SagaContext<string, BulkOperationState<TResult, TWork>> context)
    {
        if (IsComplete(context.State.RemainingWork))
        {
            context.CompleteSaga();
            await OnCompletion();
        }
        else
        {
            await _commandSender.Send(CreateCommand());
        }
        return context.State.Result;
    }

    public static void ConfigureSaga(SagaBuilder<string, BulkOperationState<TResult, TWork>> saga)
    {
        saga.OnRequest<TStartCommand, TResult>()
            .CorrelateWith(_ => Id)
            .InitializeWith<TSelf>((controller, id, command) => controller
                .Prepare(command)
                .ThenMap(rs => new BulkOperationState<TResult, TWork>(rs.Item1, rs.Item2)))
            .HandleWith<TSelf>((c, _, s) => c.Start(s));

        saga.OnRequest<TBatchCommand>()
            .CorrelateWith(_ => Id)
            .HandleWith<TSelf>((c, b, s) => c.Handle(b, s));
    }

    protected abstract bool IsComplete(TWork remainingWork);

    protected abstract Task<Result<(TResult, TWork)>> Prepare(TStartCommand command);

    protected abstract Task<TWork> HandleBatch(TBatchCommand command, TWork remainingWork);

    protected abstract TBatchCommand CreateCommand();

    protected virtual Task OnCompletion() => Task.CompletedTask;

    public static string Id => typeof(TSelf).Name;

    public static Task<bool> IsInProgress(ISagaManager sagaManager) =>
        sagaManager.IsInProgress<string, BulkOperationState<TResult, TWork>>(Id);
}
