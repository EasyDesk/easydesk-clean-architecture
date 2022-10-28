using EasyDesk.CleanArchitecture.Application.Data;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Steps;

internal class UnitOfWorkStep : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        var unitOfWorkProvider = context.GetService<IUnitOfWorkProvider>();
        await unitOfWorkProvider.RunTransactionally(() => next());
    }
}
