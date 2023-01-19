using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Messaging;

namespace EasyDesk.CleanArchitecture.Application.Sagas.BulkOperations;

public abstract record AbstractBulkOperationCommand(Guid OperationId) : IIncomingCommand, IOutgoingCommand
{
    public static string GetDestination(RoutingContext context) => context.Self;
}

public record BulkOperationState<R, S>(R Result, S RemainingWork);

public abstract class AbstractSequentialBulkOperation<TSelf, TStartCommand, TResult, TWork, TBatchCommand> :
    ISagaController<Guid, BulkOperationState<TResult, TWork>>
    where TStartCommand : IDispatchable<TResult>
    where TBatchCommand : AbstractBulkOperationCommand
    where TSelf : AbstractSequentialBulkOperation<TSelf, TStartCommand, TResult, TWork, TBatchCommand>
{
    private readonly ICommandSender _commandSender;

    public AbstractSequentialBulkOperation(ICommandSender commandSender)
    {
        _commandSender = commandSender;
    }

    private async Task<Result<TResult>> Start(SagaContext<Guid, BulkOperationState<TResult, TWork>> context) =>
        await RequestNextBatchComputation(context);

    private async Task<Result<Nothing>> Handle(TBatchCommand command, SagaContext<Guid, BulkOperationState<TResult, TWork>> context)
    {
        var remainingWork = await HandleBatch(command, context.State.RemainingWork);
        context.MutateState(s => s with { RemainingWork = remainingWork });
        return await RequestNextBatchComputation(context);
    }

    private async Task<Result<TResult>> RequestNextBatchComputation(SagaContext<Guid, BulkOperationState<TResult, TWork>> context)
    {
        if (IsComplete(context.State.RemainingWork))
        {
            context.CompleteSaga();
            await OnCompletion();
        }
        else
        {
            await _commandSender.Send(CreateCommand(context.Id));
        }
        return context.State.Result;
    }

    public static void ConfigureSaga(SagaBuilder<Guid, BulkOperationState<TResult, TWork>> saga)
    {
        saga.On<TStartCommand, TResult>()
            .CorrelateWith(_ => Guid.NewGuid())
            .InitializeWith<TSelf>(async (controller, id, command) =>
            {
                var (result, state) = await controller.Prepare(command);
                return new BulkOperationState<TResult, TWork>(result, state);
            })
            .HandleWith<TSelf>((c, _, s) => c.Start(s));

        saga.On<TBatchCommand>()
            .CorrelateWith(c => c.OperationId)
            .HandleWith<TSelf>((c, b, s) => c.Handle(b, s));
    }

    protected abstract bool IsComplete(TWork remainingWork);

    protected abstract Task<(TResult, TWork)> Prepare(TStartCommand command);

    protected abstract Task<TWork> HandleBatch(TBatchCommand command, TWork remainingWork);

    protected abstract TBatchCommand CreateCommand(Guid operationId);

    protected virtual Task OnCompletion() => Task.CompletedTask;
}
