using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Sagas.Builder;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Sagas.BulkOperations;

public abstract record AbstractBulkOperationCommand : IIncomingCommand, IOutgoingCommand
{
    public static string GetDestination(RoutingContext context) => context.Self;
}

public record BulkOperationState<R, S>(R Result, S RemainingWork);

public record BulkConcurrencyError(string OperationType) : ApplicationError
{
    public override string GetDetail() => $"Another bulk operation of type {OperationType} is already in progress.";

    public static BulkConcurrencyError FromType<T>() => new(typeof(T).Name);
}

public abstract class AbstractSequentialBulkOperation<TSelf, TStartCommand, TResult, TWork, TBatchCommand> :
    ISagaController<string, BulkOperationState<TResult, TWork>>
    where TSelf : AbstractSequentialBulkOperation<TSelf, TStartCommand, TResult, TWork, TBatchCommand>
    where TStartCommand : IDispatchable<TResult>
    where TBatchCommand : AbstractBulkOperationCommand
{
    private readonly ICommandSender _commandSender;

    protected AbstractSequentialBulkOperation(ICommandSender commandSender)
    {
        _commandSender = commandSender;
    }

    private async Task<Result<TResult>> Start(SagaContext<string, BulkOperationState<TResult, TWork>> context)
    {
        if (!context.IsNew)
        {
            return BulkConcurrencyError.FromType<TSelf>();
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

        saga.OnRequest<TBatchCommand, Nothing>()
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
