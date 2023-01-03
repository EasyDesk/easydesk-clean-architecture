using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using Rebus.Messages;
using Rebus.Routing;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;

internal class TestRouterWrapper : IRouter
{
    private readonly IRouter _defaultRouter;
    private readonly RebusEndpoint _endpoint;

    public TestRouterWrapper(IRouter defaultRouter, RebusEndpoint endpoint)
    {
        _defaultRouter = defaultRouter;
        _endpoint = endpoint;
    }

    public async Task<string> GetDestinationAddress(Message message)
    {
        if (message.Body is IIncomingCommand)
        {
            return _endpoint.InputQueueAddress;
        }
        return await _defaultRouter.GetDestinationAddress(message);
    }

    public Task<string> GetOwnerAddress(string topic) => _defaultRouter.GetOwnerAddress(topic);
}
