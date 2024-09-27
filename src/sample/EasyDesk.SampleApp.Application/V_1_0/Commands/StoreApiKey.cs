using EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.V_1_0.Dto;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

[AllowUnknownAgent]
public record StoreApiKey : ICommandRequest<Nothing>, IOverrideMultitenantPolicy
{
    public required string ApiKey { get; init; }

    public required AgentDto Agent { get; init; }

    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.AnyTenantOrPublic();
}

public class StoreApiKeyHandler : IHandler<StoreApiKey>
{
    private readonly IApiKeysStorage _apiKeysStorage;

    public StoreApiKeyHandler(IApiKeysStorage apiKeysStorage)
    {
        _apiKeysStorage = apiKeysStorage;
    }

    public async Task<Result<Nothing>> Handle(StoreApiKey request)
    {
        await _apiKeysStorage.StoreApiKey(request.ApiKey, request.Agent.ToDomainObject());
        return Ok;
    }
}
