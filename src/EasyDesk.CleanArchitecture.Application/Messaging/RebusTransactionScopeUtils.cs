using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

internal static class RebusTransactionScopeUtils
{
    public static RebusTransactionScope CreateScopeWithServiceProvider(IServiceProvider serviceProvider)
    {
        var scope = new RebusTransactionScope();
        scope.TransactionContext.SetServiceProvider(serviceProvider);
        return scope;
    }
}
