﻿using Microsoft.Extensions.DependencyInjection;
using Rebus.Pipeline;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

internal class ServiceScopeOpeningStep : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        using (var scope = context.Load<IServiceProvider>().CreateScope())
        {
            context.Save(scope);
            context.Load<ITransactionContext>().SetServiceProvider(scope.ServiceProvider);
            await next();
        }
    }
}