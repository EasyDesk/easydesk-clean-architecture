using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using Rebus.Messages;
using Rebus.Routing;
using System.Diagnostics;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Routing;

internal class OutgoingCommandRouter : IRouter
{
    private readonly RoutingContext _routingContext;

    public OutgoingCommandRouter(RebusEndpoint endpoint)
    {
        _routingContext = new(endpoint.InputQueueAddress);
    }

    public Task<string?> GetDestinationAddress(Message message) =>
        Task.FromResult(GetDestinationAddressSync(message));

    private string? GetDestinationAddressSync(Message message)
    {
        if (message.Body is not IOutgoingCommand)
        {
            throw new InvalidOperationException($"Routing a message that is not an instance of {nameof(IOutgoingCommand)}.");
        }
        var messageType = message.Body.GetType();
        return typeof(OutgoingCommandRouter)
            .GetMethod(nameof(GetDestinationAddressFromType), BindingFlags.NonPublic | BindingFlags.Instance)
            ?.MakeGenericMethod(messageType)
            .Invoke(this, null) as string;
    }

    private string GetDestinationAddressFromType<T>() where T : IOutgoingCommand =>
        T.GetDestination(_routingContext);

    public Task<string> GetOwnerAddress(string topic) =>
        Task.FromException<string>(new UnreachableException(
            $"Cannot get owner address from {nameof(OutgoingCommandRouter)}"));
}
