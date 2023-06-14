using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.SampleApp.Application.V_1_0.OutgoingEvents;
using NodaTime;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

[AllowUnknownIdentity]
public record CancellableRequest(Duration WaitTime) : ICommandRequest<Nothing>, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.IgnoreAndUsePublic();
}

public class CancellableRequestHandler : IHandler<CancellableRequest>
{
    private readonly IContextProvider _contextProvider;
    private readonly IEventPublisher _publisher;

    public CancellableRequestHandler(IContextProvider contextProvider, IEventPublisher publisher)
    {
        _contextProvider = contextProvider;
        _publisher = publisher;
    }

    public async Task<Result<Nothing>> Handle(CancellableRequest request)
    {
        await Task.Delay(request.WaitTime.ToTimeSpan(), _contextProvider.CancellationToken);
        await _publisher.Publish(new CancellationFailed());
        return Ok;
    }
}
