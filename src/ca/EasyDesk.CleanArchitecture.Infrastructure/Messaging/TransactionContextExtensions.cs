using Autofac;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Tasks;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public static class TransactionContextExtensions
{
    private const string ComponentContextKey = "x-component-context";

    public static void SetComponentContext(this ITransactionContext transactionContext, IComponentContext componentContext) =>
        transactionContext.GetOrAdd(ComponentContextKey, () => componentContext);

    public static IComponentContext GetComponentContext(this ITransactionContext transactionContext) =>
        transactionContext.GetOrThrow<IComponentContext>(ComponentContextKey);

    public static async Task<R> UseComponentContext<R>(this ITransactionContext transactionContext, IComponentContext componentContext, AsyncFunc<R> action)
    {
        var componentContextBefore = transactionContext.Items.GetOption(ComponentContextKey);

        transactionContext.Items[ComponentContextKey] = componentContext;
        try
        {
            return await action();
        }
        finally
        {
            componentContextBefore.Match(
                some: c => transactionContext.Items[ComponentContextKey] = c,
                none: () => transactionContext.Items.Remove(ComponentContextKey, out var _));
        }
    }

    public static async Task UseComponentContext(this ITransactionContext transactionContext, IComponentContext componentContext, AsyncAction action)
    {
        await transactionContext.UseComponentContext(componentContext, () => ReturningNothing(action));
    }

    public static RebusTransactionScope CreateRebusTransactionScope(this IComponentContext context)
    {
        var scope = new RebusTransactionScope();
        scope.TransactionContext.SetComponentContext(context);
        return scope;
    }
}
