using EasyDesk.CleanArchitecture.Application.Data;
using Rebus.Pipeline;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Steps;

internal class TransactionStep : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        var transactionManager = context.GetScopedService<ITransactionManager>();
        await transactionManager.RunTransactionally(() => next());
    }
}
