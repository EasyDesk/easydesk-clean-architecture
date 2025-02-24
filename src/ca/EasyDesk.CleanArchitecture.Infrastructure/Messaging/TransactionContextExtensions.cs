using Autofac;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public static class TransactionContextExtensions
{
    private const string ComponentContextKey = "x-component-context";

    public static void SetComponentContext(this ITransactionContext transactionContext, IComponentContext componentContext) =>
        transactionContext.GetOrAdd(ComponentContextKey, () => componentContext);

    public static IComponentContext GetComponentContext(this ITransactionContext transactionContext) =>
        transactionContext.GetOrThrow<IComponentContext>(ComponentContextKey);
}
