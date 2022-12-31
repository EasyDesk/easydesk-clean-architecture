using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Messaging;

namespace EasyDesk.CleanArchitecture.Application.Sagas.BulkOperations;

public abstract record AbstractBulkOperationCommand(Guid OperationId) : IIncomingCommand, IOutgoingCommand // TODO: avoid waking up all bulk ops sagas
{
    public static string GetDestination(RoutingContext context) => context.Self;
}

public abstract class AbstractSequentialBulkOperation<TSelf, C, R, S, B> :
    ISagaController<Guid, AbstractSequentialBulkOperation<TSelf, C, R, S, B>.BulkOperationState>
    where C : IDispatchable<R>
    where B : AbstractBulkOperationCommand
    where TSelf : AbstractSequentialBulkOperation<TSelf, C, R, S, B>
{
    private readonly ICommandSender _commandSender;

    public record BulkOperationState(R Result, Option<S> State);

    public AbstractSequentialBulkOperation(ICommandSender commandSender)
    {
        _commandSender = commandSender;
    }

    private async Task<Result<R>> Start(SagaContext<Guid, BulkOperationState> context)
    {
        if (context.State.State.IsAbsent)
        {
            context.CompleteSaga();
            return Success(context.State.Result);
        }
        await _commandSender.Send(CreateCommand(context.Id));
        return Success(context.State.Result);
    }

    private async Task<Result<Nothing>> Handle(B command, SagaContext<Guid, BulkOperationState> context)
    {
        var newState = await ComputeBatch(command, context.State.State.Value);
        if (newState.IsAbsent)
        {
            context.CompleteSaga();
            return Ok;
        }
        context.MutateState(s => s with { State = newState });
        await _commandSender.Send(CreateCommand(context.Id));
        return Ok;
    }

    public static void ConfigureSaga(SagaBuilder<Guid, BulkOperationState> saga)
    {
        saga.On<C, R>()
            .CorrelateWith(_ => Guid.NewGuid())
            .InitializeWith<TSelf>(async (controller, id, command) =>
            {
                var (result, state) = await controller.Prepare(command);
                return new BulkOperationState(result, state);
            })
            .HandleWith<TSelf>((c, _, s) => c.Start(s));
        saga.On<B>()
            .CorrelateWith(c => c.OperationId)
            .HandleWith<TSelf>((c, b, s) => c.Handle(b, s));
    }

    protected abstract Task<(R, Option<S>)> Prepare(C command);

    protected abstract Task<Option<S>> ComputeBatch(B command, S state);

    protected abstract B CreateCommand(Guid operationId);
}
