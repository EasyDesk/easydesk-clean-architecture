using EasyDesk.CleanArchitecture.Application.DomainServices;
using Rebus.Messages;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

public class DomainEventHandlingStep : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        await next();
        var eventQueue = context.GetService<IDomainEventFlusher>();
        await eventQueue
            .Flush()
            .ThenIfFailure(error => throw new Exception($"Unable to handle domain event {context.Load<Message>().Body}: error is {error}"));
    }
}
