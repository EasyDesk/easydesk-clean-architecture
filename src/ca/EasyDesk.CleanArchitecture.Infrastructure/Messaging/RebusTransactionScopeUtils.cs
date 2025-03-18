using Autofac;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public static class RebusTransactionScopeUtils
{
    public static RebusTransactionScope CreateScopeWithComponentContext(IComponentContext context)
    {
        var scope = new RebusTransactionScope();
        scope.TransactionContext.SetComponentContext(context);
        return scope;
    }
}
