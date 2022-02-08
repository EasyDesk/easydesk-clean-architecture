using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Results;
using Rebus.Messages;
using Rebus.Pipeline;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Steps;

public class DomainEventHandlingStep : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        await next();
        var eventQueue = context.GetScopedService<DomainEventQueue>();
        await eventQueue
            .Flush()
            .ThenIfFailure(error => throw new Exception($"Unable to handle domain event {context.Load<Message>().Body}: error is {error}"));
    }
}
