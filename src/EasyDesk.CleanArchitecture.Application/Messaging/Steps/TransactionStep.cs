using EasyDesk.CleanArchitecture.Application.Data;
using Rebus.Pipeline;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Steps;

internal class TransactionStep : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        var unitOfWorkProvider = context.GetScopedService<IUnitOfWorkProvider>();
        await unitOfWorkProvider.RunTransactionally(() => next());
    }
}
