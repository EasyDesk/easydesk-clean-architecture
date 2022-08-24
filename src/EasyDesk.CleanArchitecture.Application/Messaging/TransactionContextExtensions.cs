using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public static class TransactionContextExtensions
{
    private const string ServiceProviderKey = "x-service-provider";

    public static void SetServiceProvider(this ITransactionContext transactionContext, IServiceProvider serviceProvider) =>
        transactionContext.GetOrAdd(ServiceProviderKey, () => serviceProvider);

    public static IServiceProvider GetServiceProvider(this ITransactionContext transactionContext) =>
        transactionContext.GetOrThrow<IServiceProvider>(ServiceProviderKey);
}
